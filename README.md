# otc-alliance

基于 **ABP Framework + Vue** 的前后端分离项目模板，后端使用 ASP.NET Core / ABP Framework，前端使用 Vue 3 / Vite / Element Plus。

关键词：`ABP Framework`、`ASP.NET Core`、`.NET 9`、`Vue 3`、`Vite`、`Element Plus`、`Entity Framework Core`、`PostgreSQL`、`NLog`、`ELK`、`按钮级权限`、`.btn 权限约束`、`自定义排序`、`高级搜索`、`事件驱动`、`RabbitMQ`、`Rebus`、`Apollo`、`Redis 分布式锁`、`MinIO`、`动态表单`、`元数据驱动`。

## 项目结构

```text
otc-alliance/
|-- otc-alliance-admin/  # Vue 3 管理端前端
`-- otc-alliance-api/    # ABP Framework 后端服务
```

## 前端技术栈

路径：`otc-alliance-admin`

- Vue 3
- Vite 5
- Element Plus
- Pinia
- Vue Router
- Axios
- Vue I18n
- Sass
- WangEditor
- NProgress
- 按钮级权限指令
- 通用表格自定义排序
- 通用表格高级搜索

## 后端技术栈

路径：`otc-alliance-api`

- .NET 9
- ASP.NET Core
- ABP Framework / ASP.NET Boilerplate
- Entity Framework Core
- PostgreSQL / Npgsql
- Swashbuckle / OpenAPI
- JWT Bearer
- SignalR
- NLog
- NLog.Targets.ElasticSearch
- ELK / Elasticsearch
- Apollo Configuration
- Redis
- MinIO
- Docker
- xUnit
- ABP 权限管理
- 按钮级权限编码约束
- 动态高级搜索
- 自定义排序查询
- RabbitMQ / Rebus 消息驱动
- ABP EventBus 事件模型

## 后端架构

```text
ClientPlatform.sln
|-- src/ClientPlatform.Core                  # 领域层、权限、多语言、基础配置
|-- src/ClientPlatform.Application           # 应用服务层
|-- src/ClientPlatform.EntityFrameworkCore   # EF Core、DbContext、数据库迁移
|-- src/ClientPlatform.Web.Core              # Web/API 公共基础设施
|-- src/ClientPlatform.Web.Host              # 管理端 API Host
|-- src/ClientPlatformUser.Application       # 用户端应用服务层
|-- src/ClientPlatformUser.Web.Host          # 用户端 API Host
|-- src/ClientPlatform.Callback.API.Host     # 回调 API Host
|-- src/ClientPlatform.Migrator              # 数据库迁移执行器
|-- Utility                                  # 公共工具库
`-- test                                     # 测试项目
```

## ABP Framework 调整点

### 1. 替换默认数据库组件

项目在 ABP Framework 的 EF Core 模块中统一注册 DbContext，并通过 `Npgsql.EntityFrameworkCore.PostgreSQL` 使用 PostgreSQL。

```csharp
// otc-alliance-api/src/ClientPlatform.EntityFrameworkCore/EntityFrameworkCore/ClientPlatformDbContextConfigurer.cs
public static class ClientPlatformDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<ClientPlatformDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<ClientPlatformDbContext> builder, DbConnection connection)
    {
        builder.UseNpgsql(connection.ConnectionString);
    }
}
```

```csharp
// otc-alliance-api/src/ClientPlatform.EntityFrameworkCore/EntityFrameworkCore/ClientPlatformEntityFrameworkModule.cs
Configuration.Modules.AbpEfCore().AddDbContext<ClientPlatformDbContext>(options =>
{
    if (options.ExistingConnection != null)
    {
        ClientPlatformDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
    }
    else
    {
        ClientPlatformDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
    }
});
```

### 2. 多语言从 XML 改为 JSON

ABP Framework 默认常见做法是 XML 资源文件，这里改为 JSON 资源文件，并通过自定义 `JsonEmbeddedFileLocalizationDictionaryProvider` 加载。

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Localization/ClientPlatformLocalizationConfigurer.cs
localizationConfiguration.Sources.Add(
    new DictionaryBasedLocalizationSource(ClientPlatformConsts.LocalizationSourceName,
        new JsonEmbeddedFileLocalizationDictionaryProvider(
            typeof(ClientPlatformLocalizationConfigurer).GetAssembly(),
            "ClientPlatform.Localization.SourceFiles"
        )
    )
);
```

```xml
<!-- otc-alliance-api/src/ClientPlatform.Core/ClientPlatform.Core.csproj -->
<EmbeddedResource Include="Localization\SourceFiles\*.json"
                  Exclude="bin\**;obj\**;**\*.xproj;packages\**;@(EmbeddedResource)" />
```

### 3. 使用 NLog 替换 Log4Net，并集成 ELK

后端 Host 使用 `Abp.Castle.NLog` 接入 ABP Framework 日志系统，NLog 配置中启用 `NLog.Targets.ElasticSearch` 写入 Elasticsearch，同时保留本地文件日志。

```csharp
// otc-alliance-api/src/ClientPlatform.Web.Host/Startup/Startup.cs
services.AddAbpWithoutCreatingServiceProvider<ClientPlatformWebHostModule>(
   options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
       f => f.UseAbpNLog().WithConfig(_hostingEnvironment.IsProduction()
           ? "nlog.Production.config"
           : "nlog.config"
       )
   ),
   removeConventionalInterceptors: false
);
```

```xml
<!-- otc-alliance-api/src/ClientPlatform.Web.Host/nlog.config -->
<extensions>
  <add assembly="NLog.Targets.ElasticSearch"/>
</extensions>

<targets async="true">
  <target name="elastic" xsi:type="BufferingWrapper" flushTimeout="5000">
    <target xsi:type="ElasticSearch" uri="http://192.168.0.11:9200/" index="_admin">
      <field name="callSite" layout="${callsite:filename=true}" />
      <field name="exception" layout="${exception:tostring}" />
      <field name="serverName" layout="${machinename}" />
      <field name="url" layout="${aspnetcore-request-url}" />
      <field name="application" layout="${applicationName}" />
    </target>
  </target>

  <target name="file" xsi:type="AsyncWrapper" queueLimit="5000" overflowAction="Discard">
    <target xsi:type="File"
            fileName="${basedir}/logs/${shortdate}.log"
            layout="${longdate} ${level:uppercase=true} ${message} ${stacktrace}" />
  </target>
</targets>

<rules>
  <logger name="*" minlevel="Info" writeTo="elastic" />
  <logger name="*" minlevel="Debug" writeTo="file" />
</rules>
```

### 4. 权限控制到按钮级

权限模型基于 ABP Framework 权限体系扩展，除了菜单、页面和接口权限外，也支持细化到页面按钮级别。按钮权限使用 `.btn` 命令/编码约束，便于在权限树、角色授权和前端按钮展示之间保持统一规则。

约定示例：

```text
Pages.Users                 # 页面权限
Pages.Users.Create.btn      # 新增按钮权限
Pages.Users.Edit.btn        # 编辑按钮权限
Pages.Users.Delete.btn      # 删除按钮权限
```

前端通过全局权限指令按权限编码控制按钮显示：

```javascript
// otc-alliance-admin/src/main.js
import { permission } from "./directives/permission";

app.directive("permission", permission);
```

```vue
<el-button v-permission="'Pages.Users.Create.btn'" type="primary">
  新增
</el-button>
```

后端通过 ABP 权限定义和角色权限接口向前端输出权限树：

```javascript
// otc-alliance-admin/src/api/permission.js
export const RoleGetAllPermissionsWithLevel = {
  url: "/api/services/app/Role/GetAllPermissionsWithLevel",
  method: "get",
};
```

### 5. 前后端配合支持自定义排序和高级搜索

前端通用表格 `CommonTable` 统一处理排序和高级搜索。页面只需要声明允许排序的字段 `sortableFields` 和允许参与高级搜索的字段 `customFilterFields`，组件会把排序条件写入 `Sorting`，把高级搜索条件写入 `CustomFilters`，再随分页查询一起提交给后端。

```vue
<!-- otc-alliance-admin/src/views/usermanagement/userlist.vue -->
<CommonTable
  :query-form="searchForm"
  :sortable-fields="['userAuthStatus', 'creationTime', 'name']"
  :custom-filter-fields="[
    { value: 'UserName', label: '用户名', dataType: 'text' },
    { value: 'Email', label: '邮箱', dataType: 'text' },
    { value: 'CreationTime', label: '创建时间', dataType: 'datetime' }
  ]"
/>
```

```javascript
// otc-alliance-admin/src/components/CommonTable/index.vue
const isSortable = (fieldName) => {
  if (!fieldName || !props.sortableFields || props.sortableFields.length === 0) {
    return false;
  }
  return props.sortableFields.includes(fieldName);
};

const handleSortChange = ({ prop, order }) => {
  const searchForm = props.queryForm || props.searchForm;

  if (!prop || !order) {
    delete searchForm.Sorting;
  } else {
    const sortOrder = order === "ascending" ? "asc" : "desc";
    searchForm.Sorting = `${prop} ${sortOrder}`;
  }

  searchForm.pageNo = 1;
  searchForm.skipCount = 0;
  handleSearch();
};
```

```javascript
// otc-alliance-admin/src/components/CommonTable/index.vue
const handleAdvancedSearchConfirm = () => {
  const validFilters = customFilters.value.filter((f) => {
    if (!f.FieldName || !f.Operator) {
      return false;
    }

    const dataType = getFieldDataType(f.FieldName);
    if (dataType === "boolean") {
      return f.Value === true || f.Value === false || f.Value === 0 || f.Value === 1;
    }
    if (dataType === "number") {
      return f.Value !== null && f.Value !== undefined && f.Value !== "" && !isNaN(Number(f.Value));
    }

    return f.Value !== null && f.Value !== undefined && f.Value !== "";
  });

  if (validFilters.length > 0) {
    actualSearchForm.value.CustomFilters = validFilters;
  } else {
    delete actualSearchForm.value.CustomFilters;
  }

  advancedSearchVisible.value = false;
  handleSearch();
};
```

后端分页 DTO 继承 ABP 的排序分页模型，并额外接收 `CustomFilters`。

```csharp
// otc-alliance-api/src/ClientPlatform.Application/UserManagement/Dot/ClientUserDto.cs
public class ClientUserPagedAndSortRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public List<CustomSearchFilter> CustomFilters { get; set; }

    public void Normalize()
    {
        if (string.IsNullOrWhiteSpace(Sorting))
        {
            Sorting = "CreationTime DESC";
        }
    }
}
```

```csharp
// otc-alliance-api/src/ClientPlatform.Application/Common/CustomSearchDto.cs
public class CustomSearchFilter
{
    public string FieldName { get; set; }
    public string Operator { get; set; } // eq, neq, contains, startswith, endswith, gt, lt, gte, lte
    public string Value { get; set; }
}
```

应用服务把固定查询、高级搜索、自定义排序和分页组合在一起：

```csharp
// otc-alliance-api/src/ClientPlatform.Application/UserManagement/ClientUserAppService.cs
var qry = _clientUserRepository.GetAllIncluding(n => n.Alliance, n => n.Merchant)
    .WhereIf(!input.UserName.IsNullOrWhiteSpace(), n => n.UserName.Contains(input.UserName))
    .WhereIf(!input.Email.IsNullOrWhiteSpace(), n => n.Email.Contains(input.Email))
    .ApplyCustomFilters(input.CustomFilters)
    .OrderBy(input.Sorting);

var count = await qry.CountAsync();
var pagedResult = await qry.PageBy(input).ToListAsync();
```

高级搜索最终通过表达式树转换为 LINQ 条件：

```csharp
// otc-alliance-api/src/ClientPlatform.Application/Common/QueryableExtensions.cs
public static IQueryable<T> ApplyCustomFilters<T>(this IQueryable<T> query, List<CustomSearchFilter> filters)
{
    if (filters == null || !filters.Any())
    {
        return query;
    }

    foreach (var filter in filters)
    {
        if (string.IsNullOrWhiteSpace(filter.FieldName) || string.IsNullOrWhiteSpace(filter.Value))
        {
            continue;
        }

        query = query.ApplyFilter(filter);
    }

    return query;
}
```

### 6. 事件驱动与异步消息处理

项目中使用 **RabbitMQ + Rebus** 处理跨流程异步任务。典型链路是：应用服务写入请求记录，等待 UnitOfWork 提交完成后投递消息，Callback Host 作为消费者从 RabbitMQ 队列取出消息，并由 Rebus Handler 继续处理后续流程。

```text
PayAppService
-> CurrentUnitOfWork.Completed
-> Rebus Publish
-> RabbitMQ
-> Callback API Host
-> IHandleMessages<T>
-> 更新请求状态 / 调用外部通道 / 继续投递下一步消息
```

消息队列配置：

```csharp
// otc-alliance-api/src/ClientPlatform.Callback.API.Host/Startup.cs
services.AddRebus(configure => configure
    .Logging(l => l.Console())
    .Transport(t => t.UseRabbitMq(
        _appConfiguration["RabbitMq:ConnectionString"],
        _appConfiguration["RabbitMq:Queue"] ?? "Callback"))
    .Routing(r => r.TypeBased().MapAssemblyOf<CreatePayChannelCustomerEto>(
        _appConfiguration["RabbitMq:Queue"] ?? "Callback"))
);

services.AutoRegisterHandlersFromAssemblyOf<CreatePayChannelCustomerHandler>();
```

消费者订阅：

```csharp
// otc-alliance-api/src/ClientPlatform.Callback.API.Host/Startup.cs
var bus = app.ApplicationServices.GetRequiredService<IBus>();
bus.Subscribe<CreatePayChannelAccountEto>().Wait();
```

生产端在事务提交后投递消息，避免数据库未提交时消费者提前处理：

```csharp
// otc-alliance-api/src/ClientPlatformUser.Application/Pay/PayAppService.cs
CurrentUnitOfWork.Completed += (sender, args) =>
{
    AsyncHelper.RunSync(() => _bus.Publish(new CreatePayChannelCustomerEto
    {
        UserId = internalUserId,
        KycApplicantId = kycApplicant.Id,
        RequestId = request.Id
    }));
};
```

消息 DTO：

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Pay/Dto/CreatePayChannelCustomerEto.cs
public class CreatePayChannelCustomerEto : EventData
{
    public int UserId { get; set; }
    public Guid KycApplicantId { get; set; }
    public Guid? RequestId { get; set; }
}

// otc-alliance-api/src/ClientPlatform.Core/Pay/Dto/CreatePayChannelAccountEto.cs
public class CreatePayChannelAccountEto : EventData
{
    public Guid RequestId { get; set; }
}
```

消费者 Handler：

```csharp
// otc-alliance-api/src/ClientPlatform.Core/RabbitMq/Handlers/CreatePayChannelCustomerHandler.cs
public class CreatePayChannelCustomerHandler :
    IHandleMessages<CreatePayChannelCustomerEto>,
    ITransientDependency
{
    [UnitOfWork]
    public virtual async Task Handle(CreatePayChannelCustomerEto message)
    {
        // 创建渠道客户，成功后继续投递账户创建消息
        await _bus.Publish(new CreatePayChannelAccountEto
        {
            RequestId = message.RequestId.Value
        });
    }
}
```

```csharp
// otc-alliance-api/src/ClientPlatform.Core/RabbitMq/Handlers/CreatePayChannelAccountHandler.cs
public class CreatePayChannelAccountHandler :
    IHandleMessages<CreatePayChannelAccountEto>,
    ITransientDependency
{
    [UnitOfWork]
    public virtual async Task Handle(CreatePayChannelAccountEto message)
    {
        // 根据 RequestId 创建渠道账户并更新请求状态
    }
}
```

代码中也保留了 ABP EventBus 的本地事件模型，例如 KYC 完成事件：

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Kyc/Events/KycVerificationCompletedEvent.cs
public class KycVerificationCompletedEvent : EventData
{
    public int UserId { get; set; }
    public Guid KycApplicantId { get; set; }
    public bool IsApproved { get; set; }
}
```

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Kyc/Events/KycVerificationCompletedEventHandler.cs
public class KycVerificationCompletedEventHandler :
    IAsyncEventHandler<KycVerificationCompletedEvent>,
    ITransientDependency
{
    public async Task HandleEventAsync(KycVerificationCompletedEvent eventData)
    {
        if (eventData.IsApproved)
        {
            await _bus.Send(new CreatePayChannelCustomerEto
            {
                UserId = eventData.UserId,
                KycApplicantId = eventData.KycApplicantId
            });
        }
    }
}
```

当前主要生效的是 RabbitMQ/Rebus 消息驱动链路；KYC 的 ABP EventBus 触发点保留在代码中，但触发调用处目前是注释状态。

## 其他技术特性

### 1. Apollo 配置中心

后端 Host 在启动阶段接入 Apollo 配置中心，将本地 `appsettings.json`、环境变量、命令行参数和 Apollo 配置统一合并到 `IConfiguration`。Apollo 扩展中支持默认 namespace、私有 namespace、JSON namespace 和本地缓存目录，便于多环境配置集中管理。

```csharp
// otc-alliance-api/src/ClientPlatform.Web.Host/Startup/Program.cs
internal static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .ConfigureAppConfiguration((host, config) =>
        {
            host.ApolloConfig(config, args);
        })
        .UseCastleWindsor(IocManager.Instance.IocContainer);
```

```csharp
// otc-alliance-api/Utility/Config/ApolloConfigExtensions.cs
var apolloconfig = config.AddApollo(apolloOptions).AddDefault();

var apolloNamespace = apolloSetings.GetValue("ApolloNamespace", string.Empty);
var apolloPrivateNamespace = apolloSetings.GetValue("ApolloPrivateNamespace", string.Empty);

if (!apolloNamespace.ToUpper().Contains("applicationJson".ToUpper()))
{
    apolloNamespace += $",applicationJson";
}

foreach (var item in namespacs)
{
    if (item.ToUpper().Contains("JSON"))
    {
        apolloconfig.AddNamespace(item, ConfigFileFormat.Json);
    }
    else
    {
        apolloconfig.AddNamespace(item);
    }
}
```

### 2. Redis 分布式锁

项目封装了基于 Redis 的分布式锁服务，使用 `SET NX` 获取锁，并通过 Lua 脚本按锁值释放，避免误删其他实例持有的锁。该能力可用于消息消费幂等、并发处理保护和重复提交控制。

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Infrastructure/RedisDistributedLockService.cs
public async Task<IDisposable> AcquireLockAsync(string key, TimeSpan expiry)
{
    var lockValue = Guid.NewGuid().ToString();
    var lockKey = $"DistributedLock:{key}";

    var acquired = await _redis.StringSetAsync(
        lockKey,
        lockValue,
        expiry,
        When.NotExists
    );

    if (!acquired)
    {
        return null;
    }

    return new RedisLock(_redis, lockKey, lockValue, Logger);
}
```

```csharp
// 释放锁时只删除当前实例持有的锁
var script = @"
    if redis.call('get', KEYS[1]) == ARGV[1] then
        return redis.call('del', KEYS[1])
    else
        return 0
    end";

_redis.ScriptEvaluate(script, new RedisKey[] { _key }, new RedisValue[] { _value });
```

### 3. MinIO 对象存储封装

后端封装了 MinIO 对象存储服务，统一处理 bucket 检测、文件上传、对象读取、对象名称生成和公网访问地址拼接。应用层只需要调用统一服务即可完成文件存储，避免在业务代码中散落对象存储细节。

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Web/ServiceMinIo.cs
public ServiceMinIo(IConfiguration configuration)
{
    var endpoint = _configuration.GetSection("MinIO:Endpoint").Value;
    var accessKey = _configuration.GetSection("MinIO:AccessKey").Value;
    var secretKey = _configuration.GetSection("MinIO:SecretKey").Value;
    var withSSL = _configuration.GetSection("MinIO:WithSSL").Value == "true";

    _minio = new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        .Build();

    if (withSSL)
    {
        _minio = _minio.WithSSL();
    }
}
```

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Web/ServiceMinIo.cs
public async Task UploadFile(Stream data, string bucketName, string objectName, string contentType = "image/jpeg")
{
    bool found = await _minio.BucketExistsAsync(
        new BucketExistsArgs().WithBucket(bucketName)
    );

    if (!found)
    {
        await _minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
    }

    await _minio.PutObjectAsync(new PutObjectArgs()
        .WithBucket(bucketName)
        .WithObject(objectName)
        .WithStreamData(data)
        .WithObjectSize(data.Length)
        .WithContentType(contentType));
}
```

### 4. 通道化与元数据驱动设计

后端在外部能力接入上使用 `Factory + Provider` 的通道化设计。不同通道通过统一接口接入，应用层通过工厂解析具体 Provider；字段要求通过元数据接口返回，前端可以根据字段定义生成动态表单，减少硬编码。

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Kyc/Channels/KycChannelFactory.cs
public IKycChannelProvider GetProvider(string channelName)
{
    var providers = _iocManager.ResolveAll<IKycChannelProvider>();
    var provider = providers.FirstOrDefault(p => p.ChannelName == channelName);

    if (provider == null)
    {
        throw new UserFriendlyException($"KYC channel provider '{channelName}' was not found.");
    }

    return provider;
}
```

```csharp
// otc-alliance-api/src/ClientPlatform.Core/Pay/IChannelMetadataProvider.cs
public interface IChannelMetadataProvider
{
    Task<List<FieldDefinition>> GetRequiredFieldsAsync(
        string currency,
        string channelCode,
        PayMerchantOption merchantOption = null,
        string customerId = null
    );
}
```

### 5. 动态表单数据模型

项目使用 `DynamicForm` 保存动态 JSON 字段，可用于表单字段、请求参数、回调参数等非固定结构数据。结合通道元数据能力，可以在不频繁调整数据库字段的情况下支持不同表单结构。

```csharp
// otc-alliance-api/src/ClientPlatform.Core/DynamicForm.cs
public class DynamicForm : FullAuditedEntity<int>
{
    public int ReferenceId { get; set; }

    public string DyFormJson { get; set; }

    public FormJosnType FormJosnType { get; set; }
}
```

### 6. 前端统一时区处理

前端在 Axios 请求/响应拦截器中统一处理时间字段，请求发送前将本地时间转换为 UTC，响应返回后再按用户选择的时区转换显示。页面层不需要重复写时间转换逻辑，适合多时区后台系统。

```javascript
// otc-alliance-admin/src/utils/request.js
service.interceptors.request.use((config) => {
  const timeZoneValue = localStorage.getItem("userTimezone");
  const timezoneOffset = timeZoneValue ? timeZoneValue : 0;

  if (config.data) {
    config.data = processRequestData(config.data, timezoneOffset);
  }

  if (config.params) {
    config.params = processRequestData(config.params, timezoneOffset);
  }

  return config;
});
```

```javascript
// otc-alliance-admin/src/utils/request.js
service.interceptors.response.use((response) => {
  const res = response.data;

  if (res.result) {
    const timeZoneValue = localStorage.getItem("userTimezone");
    let timezoneOffset = timeZoneValue ? timeZoneValue : 0;
    if (timezoneOffset != 0) {
      timezoneOffset = timezoneOffset * -1;
    }
    res.result = processResponseData(res.result, timezoneOffset);
  }

  return res;
});
```

### 7. 通用后台组件沉淀

前端沉淀了 `CommonTable`、`CommonForm`、`DetailPage`、`JsonEditor`、`RichTextEditor` 等通用组件。表格组件统一封装分页、搜索、高级搜索、自定义排序、按钮权限、插槽扩展等能力，表单和详情组件用于降低后台页面重复开发成本。

```text
otc-alliance-admin/src/components/
|-- CommonTable      # 通用表格：分页、搜索、高级搜索、排序、权限按钮
|-- CommonForm       # 通用表单
|-- DetailPage       # 通用详情页
|-- JsonEditor       # JSON 编辑器
|-- RichTextEditor   # 富文本编辑器
`-- security         # 安全验证相关组件
```

### 8. Swagger、SignalR 与接口基础设施

后端 Host 集成 Swagger/OpenAPI、JWT Bearer 鉴权说明、XML 注释展示和 SignalR。Swagger UI 使用自定义嵌入资源，便于接口调试；SignalR Hub 映射为后续实时通知能力预留基础设施。

```csharp
// otc-alliance-api/src/ClientPlatform.Web.Host/Startup/Startup.cs
services.AddSignalR();
ConfigureSwagger(services);

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<AbpCommonHub>("/signalr");
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
});
```

```csharp
// otc-alliance-api/src/ClientPlatform.Web.Host/Startup/Startup.cs
options.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme()
{
    Description = "JWT Authorization header using the Bearer scheme.",
    Name = "Authorization",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.ApiKey
});
```

## 常用命令

```bash
# 前端构建
cd otc-alliance-admin
npm run build

# 后端构建
cd otc-alliance-api
dotnet build ClientPlatform.sln
```
