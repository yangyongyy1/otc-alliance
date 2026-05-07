using Abp.AspNetCore.Dependency;
using Abp.Dependency;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Com.Ctrip.Framework.Apollo;
using Utility.Config;

namespace ClientPlatform.Web.Host.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            CreateHostBuilder(args).Build().Run();
        }

        internal static IHostBuilder CreateHostBuilder(string[] args) =>
            Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((host, config) =>
                {
                    host.ApolloConfig(config, args);
                })
                .UseCastleWindsor(IocManager.Instance.IocContainer);

    }
}
