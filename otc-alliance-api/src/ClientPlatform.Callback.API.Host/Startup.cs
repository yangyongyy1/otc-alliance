using Abp.AspNetCore;
using StackExchange.Redis;
using Abp.AspNetCore.Mvc.Antiforgery;
using Abp.Castle.Logging.NLog;
using Castle.Facilities.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using ClientPlatform.Configuration;
using ClientPlatform.Identity;
using ClientPlatform.Pay;
using Utility;
using Rebus.Config;
using Rebus.ServiceProvider;
using ClientPlatform.RabbitMq.Handlers;
using ClientPlatform.Pay.Dto;
using Rebus.Routing.TypeBased;
using Rebus.Bus;

namespace ClientPlatform.Callback.API.Host
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
            // MVC
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AbpAutoValidateAntiforgeryTokenAttribute());
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            // Register Redis ConnectionMultiplexer
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfigSection = _appConfiguration.GetSection("RedisCache");
                var connectionString = redisConfigSection["ConnectionString"];
                var databaseId = redisConfigSection["DatabaseId"];

                if (string.IsNullOrEmpty(connectionString))
                {
                    // Fallback to old config or throw
                    connectionString = _appConfiguration["Redis:ConnectionString"];
                    if (string.IsNullOrEmpty(connectionString))
                        connectionString = _appConfiguration["Abp:RedisCache:ConnectionString"];

                    if (string.IsNullOrEmpty(connectionString))
                        throw new InvalidOperationException("RedisCache:ConnectionString is not configured in appsettings.json");
                }

                // Append DatabaseId if present and not already in connection string
                if (!string.IsNullOrEmpty(databaseId) && !connectionString.Contains("defaultDatabase", StringComparison.OrdinalIgnoreCase))
                {
                    connectionString = $"{connectionString},defaultDatabase={databaseId}";
                }

                return ConnectionMultiplexer.Connect(connectionString);
            });

            IdentityRegistrar.Register(services);

            // Configure CORS
            services.AddCors(
                options => options.AddPolicy(
                    _defaultCorsPolicyName,
                    builder => builder
                        .WithOrigins(
                            _appConfiguration["App:CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                )
            );

            // RabbitMQ (Resolved lazily in HostedService, automatically registered by ABP Module)
            // No manual registration needed here to avoid conflict/duplication errors.

            services.Configure<PayClientOptions>(_appConfiguration.GetSection("PayClientConfig"));

            // Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(_apiVersion, new OpenApiInfo
                {
                    Version = _apiVersion,
                    Title = "Callback API",
                    Description = "Callback API Host for External Service Callbacks (e.g. Sumsub)",
                });
                options.DocInclusionPredicate((docName, description) => true);
            });

            // Rebus Configuration (Consumer)
            services.AddRebus(configure => configure
                .Logging(l => l.Console())
                .Transport(t => t.UseRabbitMq(_appConfiguration["RabbitMq:ConnectionString"], _appConfiguration["RabbitMq:Queue"] ?? "Callback"))
                .Routing(r => r.TypeBased().MapAssemblyOf<CreatePayChannelCustomerEto>(_appConfiguration["RabbitMq:Queue"] ?? "Callback"))
            );

            // Auto Register Rebus Handlers
            services.AutoRegisterHandlersFromAssemblyOf<CreatePayChannelCustomerHandler>();

            // ABP
            services.AddAbpWithoutCreatingServiceProvider<ClientPlatformCallbackApiHostModule>(
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpNLog().WithConfig(_hostingEnvironment.IsProduction()
                        ? "nlog.Production.config"
                        : "nlog.config"
                    )
                )
            );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            var bus = app.ApplicationServices.GetRequiredService<IBus>();
            // 移除重复订阅，防止同一消息被两个服务同时消费
            // bus.Subscribe<CreatePayChannelCustomerEto>().Wait();
            bus.Subscribe<CreatePayChannelAccountEto>().Wait();

            app.UseAbp(options => { options.UseAbpRequestLocalization = false; });

            app.UseCors(_defaultCorsPolicyName);

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{_apiVersion}/swagger.json", $"Callback API {_apiVersion}");
            });
        }
    }
}
