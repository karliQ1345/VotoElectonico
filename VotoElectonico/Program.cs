using Microsoft.EntityFrameworkCore;
using VotoElectonico.Data;
using VotoElectonico.Options;
using VotoElectonico.Services.Auth;
using VotoElectonico.Services.Email;

namespace VotoElectonico
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            builder.Services.AddHttpClient();

            // Services
            builder.Services.AddScoped<IEmailSender, BrevoEmailSender>();
            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("DefaultCors");
            app.MapControllers();
            app.Run();
        }
    }
}
