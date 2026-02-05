using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using VotoElectonico.Data;
using VotoElectonico.Options;
using VotoElectonico.Services.Auth;
using VotoElectonico.Services.Email;
using Serilog;
using Serilog.Context;
using Microsoft.AspNetCore.HttpOverrides;

namespace VotoElectonico
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((ctx, lc) =>
            {
                lc.ReadFrom.Configuration(ctx.Configuration);
            });

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // CORS
            builder.Services.AddCors(o =>
            {
                o.AddPolicy("DefaultCors", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            // Db (SplitQuery para bajar el warning)
            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            {
                var cs = builder.Configuration.GetConnectionString("DefaultConnection");
                opt.UseNpgsql(cs, npgsql =>
                {
                    npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });

            // Options
            builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection("Brevo"));
            builder.Services.Configure<TwoFactorOptions>(builder.Configuration.GetSection("TwoFactor"));
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

            // HttpClient
            builder.Services.AddHttpClient();

            // Services (Email + TwoFactor + Token)
            builder.Services.AddScoped<IEmailSender, BrevoEmailSender>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            // AUTH JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // en dev OK, en prod ideal true
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,

                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                        ),

                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier,

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "VotoElectonico",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Pega: Bearer {tu_token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            builder.Services.AddAuthorization();

            // Forwarded Headers config (Render/Azure)
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;

                // Si NO pones esto, a veces no toma el header cuando está detrás de proxy.
                // Render/Azure manejan proxies dinámicos, así que esto evita drama.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            var app = builder.Build();

            // Forwarded headers DEBE ir temprano, antes de leer RemoteIpAddress y antes del logging
            app.UseForwardedHeaders();

            // Serilog request logging (después de forwarded headers)
            app.UseSerilogRequestLogging(opts =>
            {
                opts.EnrichDiagnosticContext = (diag, http) =>
                {
                    diag.Set("RequestMethod", http.Request.Method);
                    diag.Set("RequestPath", http.Request.Path.Value ?? "");
                };
            });

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("DefaultCors");

            // Middleware de auditoría (Ip/UserAgent/UserId/Role)
            // Después de ForwardedHeaders para que RemoteIpAddress sea la IP real.
            app.Use(async (ctx, next) =>
            {
                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "";
                var ua = ctx.Request.Headers.UserAgent.ToString();

                var userId = ctx.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? ctx.User?.FindFirst("sub")?.Value
                             ?? "";

                var role = ctx.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

                using (LogContext.PushProperty("Ip", ip))
                using (LogContext.PushProperty("UserAgent", ua))
                using (LogContext.PushProperty("UserId", userId))
                using (LogContext.PushProperty("Role", role))
                {
                    await next();
                }
            });

            // Middleware JWT (orden importante)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            try
            {
                app.Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
