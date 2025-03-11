
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System;
using System.Text;
using Wavlo.Data;
using Microsoft.AspNet.SignalR.Hubs;
using Wavlo;
using Wavlo.MailService;
using Wavlo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Wavlo.Models;

namespace Wavlo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddIdentity<User, IdentityRole>()
                            .AddEntityFrameworkStores<ChatDbContext>()
                            .AddDefaultTokenProviders();

            builder.Services.AddScoped<UserManager<User>>();
            builder.Services.AddScoped<SignInManager<User>>();


            builder.Services.AddDbContext<ChatDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            var emailconfig = builder.Configuration.GetSection("EmailConfigration").Get<EmailConfigration>();
            builder.Services.AddSingleton(emailconfig);

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddTransient<ITokenService, TokenService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IFileService, FileService>();


            builder.Services.AddSignalR();
            var jwtOptions = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtOptions);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
                    };

                   
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapScalarApiReference();
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); 
            app.UseRouting(); 
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<ChatHub>("/hubs/chat");


            app.MapGet("/", async (context) =>
            {
                context.Response.Redirect("/chat.html");
            });

            app.Run();
        }
    }
}
