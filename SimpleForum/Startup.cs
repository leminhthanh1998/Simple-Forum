using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleForum.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleForum.Services;
using SimpleForum.Data.Models;
using System;
using System.Threading.Tasks;
using SimpleForum.Helper;
using WebEssentials.AspNetCore.Pwa;
using WebEssentials.AspNetCore.OutputCaching;
using WebMarkupMin.AspNetCore2;
using WebMarkupMin.Core;

using IWmmLogger = WebMarkupMin.Core.Loggers.ILogger;
using WmmNullLogger = WebMarkupMin.Core.Loggers.NullLogger;
using Microsoft.AspNetCore.Identity.UI.Services;
using SimpleForum.Hubs;
using SimpleForum.Helper.Recapcha;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using AspNetCoreRateLimit;

namespace SimpleForum
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(cfg =>
            {
                cfg.SignIn.RequireConfirmedEmail = true;
                cfg.User.RequireUniqueEmail = true;

            })
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.Configure<IdentityOptions>(option =>
            {
                option.Password.RequireDigit = false;
                option.Password.RequiredUniqueChars = 0;
                option.Password.RequireNonAlphanumeric = false;
                option.Password.RequireUppercase = false;
                option.Password.RequireLowercase = false;
            });


            services.AddScoped<IForum, ForumService>();
            services.AddScoped<IPost, PostService>();
            services.AddScoped<IPostReply, PostReplyService>();
            services.AddScoped<IApplicationUser, ApplicationUserService>();
            services.AddScoped<IPostFormatter, PostFormatter>();
            services.AddScoped<IFriendlyUrl, FriendlyUrlHelper>();
            services.AddScoped<IEmailSender, EmailSender>();

            //Dang nhap bang Facebook va Google
            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["FacebookLoginAppID"];
                facebookOptions.AppSecret = Configuration["FacebookLoginAppSecret"];
            })
            .AddGoogle(googleoptions =>
            {
                googleoptions.ClientId = Configuration["GoogleLoginClientID"];
                googleoptions.ClientSecret = Configuration["GoogleLoginClientSecret"];
            });

            //Them PWA
            services.AddProgressiveWebApp();

            //Caching
            services.AddOutputCaching(options =>
            {
                options.Profiles["default"] = new OutputCacheProfile
                {
                    Duration = 120
                };
            });

            services.AddWebMarkupMin(options =>
                {
                    options.AllowMinificationInDevelopmentEnvironment = true;
                    options.DisablePoweredByHttpHeaders = true;
                })
                .AddHtmlMinification(options =>
                {
                    options.MinificationSettings.RemoveOptionalEndTags = false;
                    options.MinificationSettings.WhitespaceMinificationMode = WhitespaceMinificationMode.Safe;
                });

            services.AddSingleton<IWmmLogger, WmmNullLogger>(); // Used by HTML minifier

            //Web Optimizer
            services.AddWebOptimizer(pipeline =>
            {
                pipeline.MinifyJsFiles();
                pipeline.MinifyCssFiles();
                pipeline.MinifyHtmlFiles();
            });

            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = false;
                hubOptions.HandshakeTimeout = TimeSpan.FromSeconds(30);
                hubOptions.KeepAliveInterval = TimeSpan.FromSeconds(15);
                hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
            });

            //Cac thiet lap cho AspNetCoreRateLimit
            //services.AddOptions();//De load configuration tu appsettings.json
            //services.AddMemoryCache();//De luu bo dem va cac ip rules
            //services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            //services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            //services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            //services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();


            services.AddSingleton(Configuration.GetSection("EmailSettings").Get<EmailSettings>());//Lay cac thong tin settings email
            services.AddSingleton(Configuration.GetSection("RecaptchaSettings").Get<RecaptchaSettings>());//Lay cac thong tin setttings recaptcha
            services.Configure<RecaptchaSettings>(Configuration.GetSection("RecaptchaSettings"));
            services.AddHttpContextAccessor();//Lay dia chi ip cua nguoi truy cap
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2).AddRazorPagesOptions(options =>
                {
                    options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                }); ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseIpRateLimiting();

            app.UseWebOptimizer();

            app.UseHttpsRedirection();
            app.UseStaticFilesWithCache();
            app.UseCookiePolicy();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseAuthentication();

            app.UseOutputCaching();
            app.UseWebMarkupMin();

            app.UseSignalR((configure) =>
            {
                var desiredTransports =
                    HttpTransportType.WebSockets |
                    HttpTransportType.LongPolling;

                configure.MapHub<NotiHub>("/notihub", (options) =>
                {
                    options.Transports = desiredTransports;
                });
                configure.MapHub<UserHub>("/userhub", (options) =>
                {
                    options.Transports = desiredTransports;
                });
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
                  );
            });

            //CreateUserRoles(service).Wait();
        }

        private async Task CreateUserRoles(IServiceProvider serviceProvider)
        {
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IdentityResult roleResult;
            //Adding Admin Role
            var roleCheck = await RoleManager.RoleExistsAsync("Admin");
            if (!roleCheck)
            {
                //create the roles and seed them to the database
                roleResult = await RoleManager.CreateAsync(new IdentityRole("Admin"));
            }
            //Assign Admin role to the main User here we have given our newly registered 
            //login id for Admin management
            ApplicationUser user = await UserManager.FindByEmailAsync("eux77307@cndps.com");
            await UserManager.AddToRoleAsync(user, "Admin");
        }
    }
}
