using GuideMe.Repositories;
using GuideMe.Utility.DBInitializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace GuideMe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();


            // add dbcontext  service
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            //for Identity

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // for=> authorize

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Main/Home/NotFoundPage";

            });


            //for email sender
            builder.Services.AddTransient<IEmailSender, EmailSender>();


            //for dbinitializer 
            builder.Services.AddScoped<IDBInitializer, DBIntializer>();
            builder.Services.AddScoped<IRepository<Visitor>, Repository<Visitor>>();
            builder.Services.AddScoped<IRepository<Guide>, Repository<Guide>>();
            builder.Services.AddScoped<IRepository<UserOTP>, Repository<UserOTP>>();
            builder.Services.AddScoped<IRepository<Trip>, Repository<Trip>>();
            builder.Services.AddScoped<IRepository<Review>, Repository<Review>>();
            builder.Services.AddScoped<IRepository<Booking>, Repository<Booking>>();
            builder.Services.AddScoped<IRepository<Payment>, Repository<Payment>>();
            builder.Services.AddScoped<IRepository<Offer>, Repository<Offer>>();
            builder.Services.AddScoped<IRepository<ContactAccess>, Repository<ContactAccess>>();



            //External login by google
            builder.Services
               .AddAuthentication()
               .AddGoogle(opt =>
               {
                   var googleAuth = builder.Configuration.GetSection("Authentication:Google");
                   opt.ClientId = googleAuth["ClientId"];
                   opt.ClientSecret = googleAuth["ClientSecret"];
                   // this tells ASP.NET to use the external login scheme for temporary sign-in
                   opt.SignInScheme = IdentityConstants.ExternalScheme;
               });


            var app = builder.Build();

            // service for => dbInitailizer 
            var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider.GetService<IDBInitializer>();
            service.Initialize();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=Main}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
