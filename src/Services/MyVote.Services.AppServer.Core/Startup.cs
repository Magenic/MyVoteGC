using Autofac;
using Autofac.Extensions.DependencyInjection;
using Csla;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyVote.BusinessObjects;
using MyVote.Data.Entities;
using MyVote.Services.AppServer.Auth;
using MyVote.Services.AppServer.Filters;
using MyVote.Services.AppServer.Models;
using Newtonsoft.Json.Serialization;
using System;

namespace MyVote.Services.AppServer
{
    public partial class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<BucketStorageName>(Configuration.GetSection("BucketStorageName"));

            services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration["ConnectionStrings:Entities"]));
             
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Cookies.ApplicationCookie.AuthenticationScheme = "ApplicationCookie";
                options.Cookies.ApplicationCookie.CookieName = "Interop";
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization();

            var builder = services.AddMvc().AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            builder.AddMvcOptions(_ => { _.Filters.Add(new UnhandledExceptionFilter()); });

            services.AddCors(_ => _.AddPolicy(Constants.CorsPolicyName, b =>
            {
                b.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new BusinessObjectsModule());
            containerBuilder.RegisterModule(new EntitiesModule());
            containerBuilder.RegisterModule(new AuthModule());
            containerBuilder.Populate(services);

            var container = containerBuilder.Build();

            ApplicationContext.DataPortalActivator = new ObjectActivator(
                container, new ActivatorCallContext());
            services.AddMvc();
            return container.Resolve<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            ConfigureAuth(app);

            app.UseCors(Constants.CorsPolicyName);
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
