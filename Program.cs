using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using System.Security.Claims;

namespace PhuKienShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "PhuKienShopAuth";
                options.DefaultSignInScheme = "PhuKienShopAuth";
            })
                .AddCookie("PhuKienShopAuth")
                .AddGoogle(options =>
                {
                    options.ClientId = "890059595195-nls0ig1u9ccgvlc1u4q73bmkn9ljikqg.apps.googleusercontent.com";
                    options.ClientSecret = "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";
                });

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<PkShopContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
            
    }
}
