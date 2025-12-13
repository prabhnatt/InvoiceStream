using InvoicingCore.Interfaces;
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
    private readonly VerificationCodeService _codeService;
    private readonly IEmailSender _emailSender;

    public AuthController(UserService userService, VerificationCodeService codeService, IEmailSender emailSender)
    {
        _userService = userService;
        _codeService = codeService;
        _emailSender = emailSender;
    }

    /****************************** Local Register and Login ******************************/

    //POST /auth/local-register
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

            //Send email verification
            var code = await _codeService.GenerateEmailVerificationCodeAsync(user, HttpContext.RequestAborted);

            //send email via IEmailSender
            var subject = "Verify your email for Invoicer";
            var body = $"Your verification code is: {code}\n\n" +
                       "This code will expire in 15 minutes.";

            await _emailSender.SendEmailAsync(user.Email, subject, body, HttpContext.RequestAborted);

            Console.WriteLine($"[DEV] Email verification code for {user.Email}: {code}");

            await SignInUserAsync(user);

            return Ok(new { userId = user.Id, email = user.Email, displayName = user.DisplayName, emailVerified = user.EmailVerified });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    //POST /auth/local-login
    [HttpPost("local-login")]
    public async Task<IActionResult> LocalLogin([FromForm] LocalLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var user = await _userService.ValidateLocalUserAsync(
            request.Email,
            request.Password,
            HttpContext.RequestAborted);

        if (user is null)
            return Unauthorized("Invalid email or password.");

        await SignInUserAsync(user);

        //Important: redirect so browser navigates with the new cookie
        return Redirect("/clients");
    }

    //POST /auth/local-verify-email
    [HttpPost("local-verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Email and code are required.");

        var result = await _codeService.ValidateCodeAsync(
            request.Email,
            purpose: "email_verification",
            code: request.Code,
            HttpContext.RequestAborted);

        if (result is null)
            return BadRequest("Invalid or expired code.");

        await _userService.MarkEmailVerifiedAsync(result.UserId, HttpContext.RequestAborted);

        var user = await _userService.FindByEmailAsync(request.Email, HttpContext.RequestAborted);
        if (user is null)
            return Ok(new { message = "Email verified." });

        //Optionally sign them in or refresh cookie
        await SignInUserAsync(user);

        return Ok(new
        {
            message = "Email verified.",
            userId = user.Id,
            email = user.Email,
            emailVerified = user.EmailVerified
        });
    }

    /****************************** Google Login ******************************/
    //GET /auth/login-google
    [HttpGet("login-google")]
    public IActionResult LoginWithGoogle(string? returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        };

        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    //GET /auth/logout
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
