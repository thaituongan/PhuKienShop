using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
namespace PhuKienShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "PhuKienShopAuthCookie";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            builder.Services.AddAuthentication("PhuKienShopAuth")
                 .AddCookie("PhuKienShopAuth", options =>
                 {
                     options.LoginPath = "/Account/Login"; // Redirect here if not authenticated
                     options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here if access denied
                 })
                 .AddGoogle(options =>
                 {
                     options.ClientId = "890059595195-nls0ig1u9ccgvlc1u4q73bmkn9ljikqg.apps.googleusercontent.com";
                     options.ClientSecret = "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";
                 });
            builder.Services.AddSignalR();

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<PkShopContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

			// Add session
			builder.Services.AddHttpContextAccessor();
			builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
			

			var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();


            app.MapHub<ChatHub>("/chathub");

            app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

        }

    }
}