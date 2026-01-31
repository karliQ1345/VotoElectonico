using VotoElect.MVC.Services;

namespace VotoElect.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // Cache para Session
            builder.Services.AddDistributedMemoryCache();

            // Session (guardar cedula, rol, token, 2FA sessionId, etc.)
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // HttpClient para la API
            var apiBase = builder.Configuration["Api:BaseUrl"] ?? "https://localhost:5058/";
            builder.Services.AddHttpClient("Api", c =>
            {
                c.BaseAddress = new Uri(apiBase);
            });

            builder.Services.ConfigureHttpJsonOptions(opts =>
            {
                opts.SerializerOptions.PropertyNameCaseInsensitive = true;
            });


            // Registrar ApiService en DI
            builder.Services.AddScoped<ApiService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

