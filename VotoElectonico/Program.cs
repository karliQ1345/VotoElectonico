using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.Options;
using VotoElectonico.Services.Auth;
using VotoElectonico.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace VotoElectonico
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // CORS
            builder.Services.AddCors(o =>
            {
                o.AddPolicy("DefaultCors", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            // Db
            builder.Services.AddDbContext<ApplicationDbContext>(opt =>
            {
                var cs = builder.Configuration.GetConnectionString("DefaultConnection");
                opt.UseNpgsql(cs);
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

            //JWT Token generator
            builder.Services.AddScoped<ITokenService, TokenService>();

            //AUTH JWT
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

                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("DefaultCors");

            //Middleware JWT (orden importante)
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
