
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
using Wavlo.Repository;
using Wavlo.CloudStorage.CloudService;
using Wavlo.CloudStorage.StorageSettings;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Asp.Versioning;

namespace Wavlo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Api Versioning
            builder.Services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-version"),
                    new MediaTypeApiVersionReader("ver"));
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddIdentity<User, IdentityRole>()
                            .AddEntityFrameworkStores<ChatDbContext>()
                            .AddDefaultTokenProviders();

            //  builder.Services.AddScoped<UserManager<User>>();
            //  builder.Services.AddScoped<SignInManager<User>>();


            builder.Services.AddDbContext<ChatDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            var emailconfig = builder.Configuration.GetSection("EmailConfigration").Get<EmailConfigration>();
            builder.Services.AddSingleton(emailconfig);

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddTransient<ITokenService, TokenService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();
            builder.Services.Configure<CloudStorageSettings>(builder.Configuration.GetSection(CloudStorageSettings.AzureStorage));

            builder.Services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<CloudStorageSettings>>().Value);

            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CloudStorageSettings>>().Value;
                return new BlobServiceClient(settings.ConnectionString);
            });



            builder.Services.AddSignalR();
            var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };


                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Headers["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

      
                app.MapScalarApiReference();
                app.MapOpenApi();
            

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
