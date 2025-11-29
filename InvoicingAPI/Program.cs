
using InvoicingAPI.Endpoints;
using InvoicingAPI.Services;
using InvoicingCore;
using InvoicingCore.Configuration;
using InvoicingCore.Services;

namespace InvoicingAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            const string FrontendCorsPolicy = "FrontendCors";

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(FrontendCorsPolicy, policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7092")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
            builder.Services.AddSingleton<MongoDbContext>();

            builder.Services.AddScoped<ClientService>();
            builder.Services.AddScoped<InvoiceService>();
            builder.Services.AddScoped<InvoiceNumberService>();
            builder.Services.AddScoped<UserService>();

            builder.Services.AddScoped<IUserContext, HeaderUserContext>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(FrontendCorsPolicy);

            app.MapClientEndpoints();
            app.MapInvoiceEndpoints();

            app.Run();

        }
    }
}
