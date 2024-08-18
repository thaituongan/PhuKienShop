using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;
using PhuKienShop.Data;
using System.Security.Claims;
using System.ServiceModel;
using NuGet.Protocol;
using PhuKienShop.Models;
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
            builder.Services.AddSignalR();

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<PkShopContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            // Đăng ký dịch vụ MessageService
           /* builder.Services.AddSingleton<MessageService>();

            // Đăng ký WCF Service
            builder.Services.AddServiceModelServices()
                            .AddServiceModelMetadata();*/
           
            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            /*app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
                serviceMetadataBehavior.HttpGetEnabled = true;
                serviceMetadataBehavior.HttpsGetEnabled = true;

                var binding = new BasicHttpBinding();

                // Lấy IServiceBuilder để đăng ký dịch vụ và endpoint
                var serviceBuilder = app.Services.GetRequiredService<IServiceBuilder>();

                // Thêm dịch vụ
                serviceBuilder.AddService<MessageService>();

                // Thêm endpoint cho dịch vụ
                serviceBuilder.AddServiceEndpoint<MessageService, IMessageService>(new BasicHttpBinding(), "/MessageService.svc");
            });*/
            app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();

			app.MapHub<ChatHub>("/chathub");

			app.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
            
    }
}
