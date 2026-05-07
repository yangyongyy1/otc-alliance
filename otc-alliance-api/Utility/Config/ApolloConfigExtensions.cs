using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;

namespace Utility.Config
{
    /// <summary>
    /// Apollo配置扩展方法
    /// </summary>
    public static class ApolloConfigExtensions
    {
        /// <summary>
        /// 配置Apollo
        /// </summary>
        /// <param name="hostingContext">hostingContext</param>
        /// <param name="config">config</param>
        /// <param name="args">args</param>
        /// <param name="func">自定义方法在添加apollo之前执行</param>
        public static void ApolloConfig(this HostBuilderContext hostingContext, IConfigurationBuilder config, string[] args, Action? func = null)
        {
            config.SetBasePath(Directory.GetCurrentDirectory());
            config.AddJsonFile($"appsettings.json", optional: true);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            config.AddEnvironmentVariables();
            config.AddCommandLine(args);

            //if (hostingContext.HostingEnvironment.IsDevelopment())
            //{
            //    Console.WriteLine("本地环境，跳过 Apollo 配置加载");
            //    func?.Invoke();
            //    return;
            //}

            var configRoot = config.Build();
            var configuration = (configRoot as IConfiguration);
            
            //Apollo连通性检查
            var apolloOptions = new ApolloOptions();
            apolloOptions.LocalCacheDir = Directory.GetCurrentDirectory();
            configRoot.GetSection("apollo").Bind(apolloOptions);

            var metaServer = apolloOptions.MetaServer ?? throw new InvalidOperationException("Apollo MetaServer is not configured");
            var uri = new Uri(metaServer);

            using (var tcpClient = new System.Net.Sockets.TcpClient())
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"环境-{hostingContext.HostingEnvironment.EnvironmentName}");
                    Console.WriteLine($"apollo:Env：{apolloOptions.Env}");
                    Console.WriteLine($"apollo:Cluster：{apolloOptions.Cluster}");
                    tcpClient.Connect(uri.Host, uri.Port);
                    Console.WriteLine($"apollo:{uri.Host}:{uri.Port}连接正常");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"环境-{hostingContext.HostingEnvironment.EnvironmentName}");
                    Console.WriteLine($"apollo:{uri.Host}:{uri.Port}连接不正常 {ex.Message}");
                    Console.ResetColor();
                }
            }

            func?.Invoke();//自定义逻辑 
            //string globalConfigName = "KlicklGlobalConf.AppSetting";
            //if (hostingContext.HostingEnvironment.IsDevelopment())
            //{
            //    globalConfigName = "VGPAYGlobalConfig.AppSetting";
            //}

            //if (apolloOptions.AppId.ToLower().Contains("sandbox"))
            //{
            //    globalConfigName = "SandboxGlobal.AppSetting";
            //}

            //if (apolloOptions.AppId.ToLower().Contains("test"))
            //{
            //    globalConfigName = "TestGlobal.AppSetting";
            //}

            //if (apolloOptions.AppId.ToLower().Contains("-v2"))
            //{
            //    globalConfigName = "KlicklGlobal-V2.AppSetting";
            //}

            var apolloconfig = config.AddApollo(apolloOptions).AddDefault();

            var apolloSetings = apolloconfig.Build();

            var apolloNamespace = apolloSetings.GetValue("ApolloNamespace", string.Empty);
            var apolloPrivateNamespace = apolloSetings.GetValue("ApolloPrivateNamespace", string.Empty);
            if (string.IsNullOrWhiteSpace(apolloNamespace))
            {
                apolloNamespace = apolloPrivateNamespace;
            }
            else
            {
                apolloNamespace += $",{apolloPrivateNamespace}";
            }
            if (!apolloNamespace.ToUpper().Contains("applicationJson".ToUpper()))
            {
                apolloNamespace += $",applicationJson";
            }
            if (!string.IsNullOrWhiteSpace(apolloNamespace))
            {
                var namespacs = apolloNamespace.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n.Trim())
                    .ToList();

                foreach (var item in namespacs)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    if (item.ToUpper().Contains("JSON"))
                    {
                        apolloconfig.AddNamespace(item, ConfigFileFormat.Json);
                    }
                    else
                    {
                        apolloconfig.AddNamespace(item);
                    }
                }
            }
            else
            {
                Console.WriteLine($"获取不到ApolloNamespace、ApolloPrivateNamespace配置 apollo:{uri.Host}:{uri.Port}连接不正常");
                Console.ResetColor();
            }
        }
    }
}

