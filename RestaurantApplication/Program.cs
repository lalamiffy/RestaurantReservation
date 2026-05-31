using RestaurantApplication.Models.Api;
using RestaurantApplication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RestaurantApplication.Data;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RestaurantApplication.Core.Repositories;
using RestaurantApplication.Core;
using System.Globalization;

namespace RestaurantApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-AU");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-AU");

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
               .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                //default Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 0;
            });

            builder.Services.AddScoped<PersonService>();

            builder.Services.Configure<MyDemoDatabaseSettings>(builder.Configuration.GetSection("MyDemoDatabase"));
            builder.Services.AddSingleton<MenuService>();
            builder.Services.AddSingleton<OrderService>();

            //builder.Services.AddControllers()
            //    .AddJsonOptions(
            //        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

            builder.Services.AddScoped<MenuService>();
            builder.Services.AddScoped<OrderService>();


            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin();
                    });
            });


            builder.Services.AddControllersWithViews();

            //builder.Services.AddAuthentication(o =>
            //{
            //    o.DefaultScheme = "JWT_OR_COOKIE";
            //    o.DefaultChallengeScheme = "JWT_OR_COOKIE";
            //})
            //    .AddJwtBearer(options =>
            //    {
            //        options.RequireHttpsMetadata = false;
            //        options.SaveToken = true;
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = true,
            //            ValidateAudience = true,
            //            ValidateLifetime = true,

            //            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            //            ValidAudience = builder.Configuration["Jwt:Audience"],
            //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

            //            // Prevents tokens without an expiry from ever working, as that would be a security vulnerability.
            //            RequireExpirationTime = true,

            //            // ClockSkew generally exists to account for potential clock difference between issuer and consumer
            //            // But we are both, so we don't need to account for it.
            //            // For all intents and purposes, this is optional
            //            ClockSkew = TimeSpan.Zero
            //        };
            //    })
            //    .AddPolicyScheme("JWT_OR_COOKIE", null, o =>
            //    {
            //        o.ForwardDefaultSelector = c =>
            //        {
            //            string auth = c.Request.Headers[HeaderNames.Authorization];
            //            if (!string.IsNullOrWhiteSpace(auth) && auth.StartsWith("Bearer "))
            //            {
            //                return JwtBearerDefaults.AuthenticationScheme;
            //            }

            //            return IdentityConstants.ApplicationScheme;
            //        };
            //    });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                string[] roleNames = { "Member", "Staff", "Manager" };

                foreach (var roleName in roleNames)
                {
                    var roleExists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
                    if (!roleExists)
                    {
                        roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                    }
                }

                var manager = userManager.FindByEmailAsync("manager@e.com").GetAwaiter().GetResult();
                if (manager == null)
                {
                    manager = new IdentityUser
                    {
                        UserName = "manager@e.com",
                        Email = "manager@e.com",
                        EmailConfirmed = true
                    };
                    userManager.CreateAsync(manager, "manager@e.com").GetAwaiter().GetResult();
                }

                if (!userManager.IsInRoleAsync(manager, "Manager").GetAwaiter().GetResult())
                {
                    userManager.AddToRoleAsync(manager, "Manager").GetAwaiter().GetResult();
                }

                var staff = userManager.FindByEmailAsync("staff@e.com").GetAwaiter().GetResult();
                if (staff == null)
                {
                    staff = new IdentityUser
                    {
                        UserName = "staff@e.com",
                        Email = "staff@e.com",
                        EmailConfirmed = true
                    };
                    userManager.CreateAsync(staff, "staff@e.com").GetAwaiter().GetResult();
                }

                if (!userManager.IsInRoleAsync(staff, "Staff").GetAwaiter().GetResult())
                {
                    userManager.AddToRoleAsync(staff, "Staff").GetAwaiter().GetResult();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            //    app.UseMigrationsEndPoint();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();



        }
    }
}
