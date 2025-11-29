using InvoicingCore.Services;
using InvoicingServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvoicingServer.Controllers;

[Route("auth")]
public class AuthController : Controller
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    /****************************** Local Register and Login ******************************/

    // POST /auth/local-register
    [HttpPost("local-register")]
    public async Task<IActionResult> LocalRegister([FromBody] LocalRegisterRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        try
        {
            var user = await _userService.CreateLocalUserAsync(
                request.Email,
                request.Password,
                request.DisplayName,
                HttpContext.RequestAborted);

            await SignInUserAsync(user);

            return Ok(new { userId = user.Id, email = user.Email, displayName = user.DisplayName });
        }
        catch (InvalidOperationException ex)
        {
            // e.g. email already exists
            return BadRequest(ex.Message);
        }
    }    

    // POST /auth/local-login
    [HttpPost("local-login")]
    public async Task<IActionResult> LocalLogin([FromBody] LocalLoginRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var user = await _userService.ValidateLocalUserAsync(
            request.Email,
            request.Password,
            HttpContext.RequestAborted);

        if (user is null)
            return Unauthorized("Invalid email or password.");

        await SignInUserAsync(user);

        return Ok(new { userId = user.Id, email = user.Email, displayName = user.DisplayName });
    }

    /****************************** Google Login ******************************/

    [HttpGet("login-google")]
    public IActionResult LoginWithGoogle(string? returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [HttpGet("denied")]
    public IActionResult AccessDenied() => Content("Access denied");

    private async Task SignInUserAsync(InvoicingCore.Models.User user)
    {
        var claims = new List<Claim>
        {
            new("userId", user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName ?? user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
    }
}
