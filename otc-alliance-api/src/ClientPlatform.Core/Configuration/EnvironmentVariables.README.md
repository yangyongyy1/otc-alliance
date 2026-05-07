# EnvironmentVariables 配置说明

## 概述

`EnvironmentVariables` 是一个环境变量配置对象，通过读取配置文件创建，支持开发、测试、沙盒、生产四个环境。该类实现了 `ISingletonDependency` 接口，可以通过依赖注入直接使用。

## 使用方法

### 1. 在类中注入使用

```csharp
using ClientPlatform.Configuration;
using Abp.Dependency;

public class MyService : ITransientDependency
{
    private readonly EnvironmentVariables _envVars;

    public MyService(EnvironmentVariables envVars)
    {
        _envVars = envVars;
    }

    public void DoSomething()
    {
        if (_envVars.IsDevelopment)
        {
            // 开发环境逻辑
        }

        var apiUrl = _envVars.ApiBaseUrl;
        var customValue = _envVars.GetCustomVariable("MyKey", "defaultValue");
    }
}
```

### 2. 配置文件示例

#### appsettings.json (基础配置)

```json
{
  "Environment": "Development",
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=ClientPlatform;User Id=sa;Password=yourpassword;"
  },
  "RedisCache": {
    "ConnectionString": "localhost:6379"
  },
  "EnvironmentVariables": {
    "ApiBaseUrl": "https://api.example.com",
    "FrontendUrl": "https://app.example.com",
    "EnableDebug": true,
    "LogLevel": "Debug",
    "EnableSwagger": true,
    "Custom": {
      "MyCustomKey": "MyCustomValue",
      "AnotherKey": "AnotherValue"
    }
  },
  "apollo": {
    "AppId": "ClientP-2",
    "MetaServer": "http://192.168.0.21:8080",
    "Cluster": "default",
    "Namespace": "application"
  }
}
```

#### appsettings.Development.json (开发环境)

```json
{
  "Environment": "Development",
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=ClientPlatform_Dev;User Id=sa;Password=devpassword;"
  },
  "RedisCache": {
    "ConnectionString": "localhost:6379"
  },
  "EnvironmentVariables": {
    "ApiBaseUrl": "http://localhost:5000",
    "FrontendUrl": "http://localhost:4200",
    "EnableDebug": true,
    "LogLevel": "Debug",
    "EnableSwagger": true,
    "Custom": {
      "FeatureFlag": "enabled",
      "TestMode": "true"
    }
  },
  "apollo": {
    "AppId": "ClientP-2-Dev",
    "MetaServer": "http://dev-apollo.example.com:8080"
  }
}
```

#### appsettings.Test.json (测试环境)

```json
{
  "Environment": "Test",
  "ConnectionStrings": {
    "Default": "Server=test-db.example.com;Database=ClientPlatform_Test;User Id=testuser;Password=testpassword;"
  },
  "RedisCache": {
    "ConnectionString": "test-redis.example.com:6379"
  },
  "EnvironmentVariables": {
    "ApiBaseUrl": "https://test-api.example.com",
    "FrontendUrl": "https://test-app.example.com",
    "EnableDebug": false,
    "LogLevel": "Information",
    "EnableSwagger": true,
    "Custom": {
      "FeatureFlag": "enabled",
      "TestMode": "true"
    }
  },
  "apollo": {
    "AppId": "ClientP-2-Test",
    "MetaServer": "http://test-apollo.example.com:8080"
  }
}
```

#### appsettings.Sandbox.json (沙盒环境)

```json
{
  "Environment": "Sandbox",
  "ConnectionStrings": {
    "Default": "Server=sandbox-db.example.com;Database=ClientPlatform_Sandbox;User Id=sandboxuser;Password=sandboxpassword;"
  },
  "RedisCache": {
    "ConnectionString": "sandbox-redis.example.com:6379"
  },
  "EnvironmentVariables": {
    "ApiBaseUrl": "https://sandbox-api.example.com",
    "FrontendUrl": "https://sandbox-app.example.com",
    "EnableDebug": false,
    "LogLevel": "Information",
    "EnableSwagger": true,
    "Custom": {
      "FeatureFlag": "enabled",
      "SandboxMode": "true"
    }
  },
  "apollo": {
    "AppId": "ClientP-2-Sandbox",
    "MetaServer": "http://sandbox-apollo.example.com:8080"
  }
}
```

#### appsettings.Production.json (生产环境)

```json
{
  "Environment": "Production",
  "ConnectionStrings": {
    "Default": "Server=prod-db.example.com;Database=ClientPlatform_Prod;User Id=produser;Password=prodpassword;"
  },
  "RedisCache": {
    "ConnectionString": "prod-redis.example.com:6379"
  },
  "EnvironmentVariables": {
    "ApiBaseUrl": "https://api.example.com",
    "FrontendUrl": "https://app.example.com",
    "EnableDebug": false,
    "LogLevel": "Warning",
    "EnableSwagger": false,
    "Custom": {
      "FeatureFlag": "disabled",
      "MaintenanceMode": "false"
    }
  },
  "apollo": {
    "AppId": "ClientP-2",
    "MetaServer": "http://prod-apollo.example.com:8080"
  }
}
```

## 配置属性说明

### 基础属性

- **Environment**: 当前环境名称（Development, Test, Sandbox, Production）
- **ConnectionString**: 数据库连接字符串
- **RedisConnectionString**: Redis连接字符串
- **ApiBaseUrl**: API基础地址
- **FrontendUrl**: 前端应用地址
- **EnableDebug**: 是否启用调试模式
- **LogLevel**: 日志级别
- **EnableSwagger**: 是否启用Swagger

### 环境判断属性

- **IsDevelopment**: 是否为开发环境
- **IsTest**: 是否为测试环境
- **IsSandbox**: 是否为沙盒环境
- **IsProduction**: 是否为生产环境

### Apollo配置

- **Apollo.AppId**: Apollo应用ID
- **Apollo.MetaServer**: Apollo Meta服务器地址
- **Apollo.LocalCacheDir**: 本地缓存目录
- **Apollo.Cluster**: 集群名称
- **Apollo.Namespace**: 命名空间

### 自定义变量

通过 `CustomVariables` 字典可以存储任意自定义环境变量，使用 `GetCustomVariable` 方法获取。

## 配置读取优先级

1. `EnvironmentVariables:Key` 格式的配置
2. 根节点的 `Key` 配置（向后兼容）
3. 默认值（根据环境自动判断）

## 注意事项

1. `EnvironmentVariables` 类实现了 `ISingletonDependency`，会被自动注册为单例
2. 构造函数需要 `IConfiguration` 参数，确保在ABP模块初始化时 `IConfiguration` 已注册
3. 如果配置项不存在，会使用合理的默认值
4. 生产环境默认关闭 Swagger 和 Debug 模式
