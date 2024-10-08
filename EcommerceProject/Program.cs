using Microsoft.EntityFrameworkCore;
using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.DataAcess.Repository;
using Microsoft.AspNetCore.Identity;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Ecommerce.DataAcess.DBInitializer;

namespace MVCProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<AppDbContext>(
            options => options.UseSqlServer(
             builder.Configuration.GetConnectionString("DefaultConn"),
             sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });
            builder.Services.AddAuthentication().AddFacebook(options =>
            {
                options.AppId = "886774686750657";
                options.AppSecret = "19cb544c4b9b524010fe18e399ed075c";
            });
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.IOTimeout = TimeSpan.FromMinutes(90);

            });
            builder.Services.AddScoped<IDBInitializer, DBInitializer>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            
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
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            SeedDatabase();
            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            app.Run();


            void SeedDatabase()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
                    dbInitializer.Initialize();
                }
            }
        } 
    
    }
}
