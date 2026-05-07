using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using ClientPlatform;

namespace ClientPlatform.Configuration
{
    /// <summary>
    /// 环境变量配置对象
    /// 通过读取配置文件创建，支持开发、测试、沙盒、生产四个环境
    /// </summary>
    public class EnvironmentVariables : ISingletonDependency
    {
        /// <summary>
        /// 当前环境名称（Development, Test, Sandbox, Production）
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// 是否为开发环境
        /// </summary>
        public bool IsDevelopment => Environment == EnvironmentConsts.Development;

        /// <summary>
        /// 是否为测试环境
        /// </summary>
        public bool IsTest => Environment == EnvironmentConsts.Test;

        /// <summary>
        /// 是否为沙盒环境
        /// </summary>
        public bool IsSandbox => Environment == EnvironmentConsts.Sandbox;

        /// <summary>
        /// 是否为生产环境
        /// </summary>
        public bool IsProduction => Environment == EnvironmentConsts.Production;

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string RedisConnectionString { get; set; }

        /// <summary>
        /// API基础地址
        /// </summary>
        public string ApiBaseUrl { get; set; }

        /// <summary>
        /// 前端应用地址
        /// </summary>
        public string FrontendUrl { get; set; }

        /// <summary>
        /// 是否启用调试模式
        /// </summary>
        public bool EnableDebug { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public string LogLevel { get; set; }

        /// <summary>
        /// 是否启用Swagger
        /// </summary>
        public bool EnableSwagger { get; set; }

        /// <summary>
        /// 自定义环境变量字典（用于存储其他环境相关的配置）
        /// </summary>
        public Dictionary<string, string> CustomVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 构造函数，通过IConfiguration绑定配置
        /// </summary>
        /// <param name="configuration">配置对象</param>
        public EnvironmentVariables(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // 读取环境名称，默认使用Production
            Environment = configuration["Environment"] ?? EnvironmentConsts.Production;
            
        }

        /// <summary>
        /// 获取自定义环境变量
        /// </summary>
        /// <param name="key">变量键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>变量值</returns>
        public string GetCustomVariable(string key, string defaultValue = null)
        {
            return CustomVariables.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 获取自定义环境变量（泛型版本）
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="key">变量键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>变量值</returns>
        public T GetCustomVariable<T>(string key, T defaultValue = default(T))
        {
            if (!CustomVariables.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
