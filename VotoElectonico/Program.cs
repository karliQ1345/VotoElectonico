
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
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<BrevoOptions>(builder.Configuration.GetSection("Brevo"));
            builder.Services.Configure<TwoFactorOptions>(builder.Configuration.GetSection("TwoFactor"));

            builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();

            builder.Services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(15);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
