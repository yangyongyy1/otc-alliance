using Abp.Dependency;
using Abp.Logging;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlatform.Web
{
    public class CustomEnvironment : ISingletonDependency
    {
        /// <summary>
        /// SecretKey
        /// </summary>   
        public ILogger Logger { get; set; }


        /// <summary>
        /// Environment
        /// </summary>   
        public string Environment { get; set; }

        public CustomEnvironment(IConfiguration configuration)
        {
            Environment = configuration.GetSection("Environment").Value ?? EnvironmentConsts.Production;
            Logger = IocManager.Instance.IsRegistered(typeof(ILoggerFactory)) ? IocManager.Instance.Resolve<ILoggerFactory>().Create(typeof(LogHelper)) : NullLogger.Instance;
            Logger.Info($"Environment===当前环境==={Environment}");
        }


        public bool IsDevelopment()
        {
            return Environment == EnvironmentConsts.Development;
        }

        public bool IsTest()
        {
            return Environment == EnvironmentConsts.Test;
        }

        public bool IsStaging()
        {
            return Environment == EnvironmentConsts.Staging;
        }

        public bool IsSandbox()
        {
            return Environment == EnvironmentConsts.Sandbox;
        }

        public bool IsProduction()
        {
            return Environment == EnvironmentConsts.Production;
        }
    }
}
