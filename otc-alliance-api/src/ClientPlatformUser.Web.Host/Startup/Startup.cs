using Abp.AspNetCore;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Castle.Logging.NLog;
using Abp.Dependency;
using Abp.Extensions;
using Abp.Json;
using Castle.Facilities.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ClientPlatform.Configuration;
using ClientPlatform.Email;
using ClientPlatform.Identity;
using ClientPlatform.Pay;
using Utility;
using Rebus.Config;
using Rebus.ServiceProvider;
using Rebus.Routing.TypeBased;
using ClientPlatform.Pay.Dto;
using ClientPlatform.Kyc.Dto;

using StackExchange.Redis;

namespace ClientPlatform.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "localhost";

        private const string _apiVersion = "v1";

        private readonly IConfiguration _appConfiguration;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Abp.Timing.Clock.Provider = Abp.Timing.ClockProviders.Utc;
            _hostingEnvironment = env;
            //_appConfiguration = env.GetAppConfiguration();
            _appConfiguration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            });

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);


            services.AddNECaptchaValidateService().Configure<NECaptchaValidateOptions>(_appConfiguration.GetSection("NECaptchaValidateConfig"));

            services.Configure<PayClientOptions>(_appConfiguration.GetSection("PayClientConfig"));

            services.Configure<MailConfig>(_appConfiguration.GetSection("MailConfig"));


            services.AddSignalR();

            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                //options.SerializerSettings.ContractResolver = new AbpMvcContractResolver(IocManager.Instance)
                //{
                //    NamingStrategy = new CamelCaseNamingStrategy()
                //};
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            // Configure CORS for angular2 UI
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            // App:CorsOrigins in appsettings.json can contain more than one address separated by comma.
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray()
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );

            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            ConfigureSwagger(services);

            // Configure Rebus (基础配置 - 只用于依赖注入解析)
            // Configure Rebus
            services.AddRebus(configure => configure
                .Logging(l => l.Console())
                .Transport(t => t.UseRabbitMq(_appConfiguration["RabbitMq:ConnectionString"], _appConfiguration["RabbitMq:Queue"] ?? "ClientPlatformUser"))
                .Routing(r => r.TypeBased().MapAssemblyOf<CreatePayChannelCustomerEto>(_appConfiguration["RabbitMq:Queue"] ?? "ClientPlatformUser"))
            );

            // Auto Register Rebus Handlers
            services.AutoRegisterHandlersFromAssemblyOf<ClientPlatform.RabbitMq.Handlers.CreatePayChannelCustomerHandler>();

            // =============== 注册 Redis 连接 ===============
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
            {
                var redisConfigSection = _appConfiguration.GetSection("RedisCache");
                var connectionString = redisConfigSection["ConnectionString"];
                var databaseId = redisConfigSection["DatabaseId"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback to old config or throw
                    connectionString = _appConfiguration["Redis:ConnectionString"];
                    if (string.IsNullOrEmpty(connectionString))
                        throw new InvalidOperationException("RedisCache:ConnectionString is not configured in appsettings.json");
                }

                // Append DatabaseId if present and not already in connection string
                if (!string.IsNullOrEmpty(databaseId) && !connectionString.Contains("defaultDatabase", StringComparison.OrdinalIgnoreCase))
                {
                    connectionString = $"{connectionString},defaultDatabase={databaseId}";
                }

                return StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
            });

            // =============== 注册分布式锁服务 ===============
            // IDistributedLockService 已通过 ITransientDependency 自动注册
            // RedisDistributedLockService 会被 ABP 自动发现并注册

            services.AddAbpWithoutCreatingServiceProvider<ClientPlatformUserWebHostModule>(
               // Configure Log4Net logging
               options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                   f => f.UseAbpNLog().WithConfig(_hostingEnvironment.IsProduction()
                       ? "nlog.Production.config"
                       : "nlog.config"
                   )
               ),
               removeConventionalInterceptors: false
           );

            ////// Configure Abp and Dependency Injection
            //services.AddAbpWithoutCreatingServiceProvider<ClientPlatformWebHostModule>(
            //    // Configure Log4Net logging
            //    //options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
            //    //    f => f.UseAbpLog4Net().WithConfig(_hostingEnvironment.IsDevelopment()
            //    //        ? "nlog.Production.config"
            //    //        : "nlog.config"
            //    //    )
            //    //)
            //);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); // Initializes ABP framework.

            app.UseCors(_defaultCorsPolicyName); // Enable CORS!

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAbpRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<AbpCommonHub>("/signalr");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute("defaultWithArea", "{area}/{controller=Home}/{action=Index}/{id?}");
            });

            var bus = app.ApplicationServices.GetRequiredService<Rebus.Bus.IBus>();
            bus.Subscribe<CreatePayChannelCustomerEto>().Wait();
            bus.Subscribe<CreatePayChannelAccountEto>().Wait();

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                // specifying the Swagger JSON endpoint.
                options.SwaggerEndpoint($"/swagger/{_apiVersion}/swagger.json", $"ClientPlatform API {_apiVersion}");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("ClientPlatformUser.Web.Host.wwwroot.swagger.ui.index.html");
                options.DisplayRequestDuration(); // Controls the display of the request duration (in milliseconds) for "Try it out" requests.
            }); // URL: /swagger
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(_apiVersion, new OpenApiInfo
                {
                    Version = _apiVersion,
                    Title = "ClientPlatform API",
                    Description = "ClientPlatform",
                    // uncomment if needed TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "ClientPlatform",
                        Email = string.Empty,
                        Url = new Uri("https://twitter.com/aspboilerplate"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://github.com/aspnetboilerplate/aspnetboilerplate/blob/dev/LICENSE.md"),
                    }
                });
                options.DocInclusionPredicate((docName, description) => true);

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                //add summaries to swagger
                bool canShowSummaries = _appConfiguration.GetValue<bool>("Swagger:ShowSummaries");
                if (canShowSummaries)
                {
                    //var hostXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    //var hostXmlPath = Path.Combine(AppContext.BaseDirectory, hostXmlFile);
                    //options.IncludeXmlComments(hostXmlPath);

                    var applicationXml = $"ClientPlatformUser.Application.xml";
                    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXml);
                    options.IncludeXmlComments(applicationXmlPath);

                    var webCoreXmlFile = $"ClientPlatform.Web.Core.xml";
                    var webCoreXmlPath = Path.Combine(AppContext.BaseDirectory, webCoreXmlFile);
                    options.IncludeXmlComments(webCoreXmlPath);

                    var entityXmlFile = $"ClientPlatform.Core.xml";
                    var entityXmlPath = Path.Combine(AppContext.BaseDirectory, entityXmlFile);
                    options.IncludeXmlComments(entityXmlPath);




                }
            });
        }
    }
}
