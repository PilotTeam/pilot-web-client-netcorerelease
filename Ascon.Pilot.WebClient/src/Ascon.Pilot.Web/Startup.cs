using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ascon.Pilot.Web.Models;
using Ascon.Pilot.Web.Models.Store;
using DocumentRender;
using DocumentRender.DocumentConverter;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ascon.Pilot.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("logger.config"));

            // Set up configuration sources.  
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json");
            this.Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddMemoryCache();
            services.AddSession();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                //options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.LoginPath = "/Account/LogIn";
                //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            //
            services.AddSingleton<IContextLifetimeService, ContextLifetimeService>();
            services.AddSingleton<IContextHolder, ContextHolder>();
            services.AddSingleton<IDocumentConverterFactory, DocumentConverterFactory>();
            services.AddSingleton<IDocumentRender, DocumentRender.DocumentRender>();
            services.AddSingleton<IStore, Store>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // When the app doesn't run in the Development environment:
                //   Enable the Exception Handler Middleware to catch exceptions
                //     thrown in the following middlewares.
                //   Use the HTTP Strict Transport Security Protocol (HSTS)
                //     Middleware.
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSession();
            app.UseStaticFiles();

            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

            });
        }
    }
}
