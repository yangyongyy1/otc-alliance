# ClientPlatform API

基于 **ABP Framework / ASP.NET Boilerplate + ASP.NET Core + Entity Framework Core** 的后端服务，面向 Vue 前端提供 API Host、用户端 Host、回调 Host 和数据库迁移工具。

关键词：`ABP Framework`、`ASP.NET Boilerplate`、`.NET 9`、`ASP.NET Core`、`Entity Framework Core`、`PostgreSQL`、`Npgsql`、`NLog`、`ELK`、`Elasticsearch`、`Vue`、`按钮级权限`、`.btn 权限约束`、`自定义排序`、`高级搜索`、`事件驱动`、`RabbitMQ`、`Rebus`。

## 技术栈

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
- Docker
- xUnit
- ABP 权限管理
- 按钮级权限编码约束
- 动态高级搜索
- 自定义排序查询
- RabbitMQ / Rebus 消息驱动
- ABP EventBus 事件模型

## 项目结构

```text
ClientPlatform.sln
|-- src/ClientPlatform.Core
|-- src/ClientPlatform.Application
|-- src/ClientPlatform.EntityFrameworkCore
|-- src/ClientPlatform.Web.Core
|-- src/ClientPlatform.Web.Host
|-- src/ClientPlatformUser.Application
|-- src/ClientPlatformUser.Web.Host
|-- src/ClientPlatform.Callback.API.Host
|-- src/ClientPlatform.Migrator
|-- Utility
`-- test
```

## ABP Framework 调整说明

### 替换默认数据库组件

数据库访问层集中在 `ClientPlatform.EntityFrameworkCore`，通过 ABP 的 `AbpEfCore().AddDbContext<T>()` 注册 DbContext，并在 `ClientPlatformDbContextConfigurer` 中切换到 PostgreSQL / Npgsql。

```csharp
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

### 多语言改为 JSON

多语言资源位于 `src/ClientPlatform.Core/Localization/SourceFiles/*.json`。项目使用自定义 `JsonEmbeddedFileLocalizationDictionaryProvider` 读取 JSON 资源，而不是 XML 资源。

```csharp
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
<EmbeddedResource Include="Localization\SourceFiles\*.json"
                  Exclude="bin\**;obj\**;**\*.xproj;packages\**;@(EmbeddedResource)" />
```

### NLog 替换 Log4Net 并集成 ELK

Host 使用 `Abp.Castle.NLog` 将 NLog 接入 ABP Framework 日志系统，NLog 通过 `NLog.Targets.ElasticSearch` 输出到 Elasticsearch，同时输出本地文件日志。

```csharp
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

### 权限控制到按钮级

权限体系基于 ABP Framework 权限管理扩展，支持菜单权限、页面权限、接口权限和按钮级权限。按钮权限统一使用 `.btn` 命令/编码约束，方便在后端权限定义、角色授权和 Vue 前端按钮展示之间保持一致。

约定示例：

```text
Pages.Users                 # 页面权限
Pages.Users.Create.btn      # 新增按钮权限
Pages.Users.Edit.btn        # 编辑按钮权限
Pages.Users.Delete.btn      # 删除按钮权限
```

前端通过全局权限指令消费后端权限编码：

```javascript
import { permission } from "./directives/permission";

app.directive("permission", permission);
```

```vue
<el-button v-permission="'Pages.Users.Create.btn'" type="primary">
  新增
</el-button>
```

权限树接口：

```javascript
export const RoleGetAllPermissionsWithLevel = {
  url: "/api/services/app/Role/GetAllPermissionsWithLevel",
  method: "get",
};
```

### 前后端配合支持自定义排序和高级搜索

Vue 前端 `CommonTable` 负责把排序和高级搜索统一转换为查询参数：

- `sortableFields`：声明哪些字段允许排序。
- `Sorting`：传递给后端的排序表达式，例如 `CreationTime desc`。
- `customFilterFields`：声明高级搜索可选字段。
- `CustomFilters`：传递给后端的高级搜索条件数组。

```javascript
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
const handleAdvancedSearchConfirm = () => {
  const validFilters = customFilters.value.filter((f) => {
    if (!f.FieldName || !f.Operator) {
      return false;
    }

    const dataType = getFieldDataType(f.FieldName);
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

后端 DTO 接收高级搜索条件：

```csharp
public class CustomSearchFilter
{
    public string FieldName { get; set; }
    public string Operator { get; set; }
    public string Value { get; set; }
}
```

应用服务组合固定查询、高级搜索、自定义排序和分页：

```csharp
var qry = _clientUserRepository.GetAllIncluding(n => n.Alliance, n => n.Merchant)
    .WhereIf(!input.UserName.IsNullOrWhiteSpace(), n => n.UserName.Contains(input.UserName))
    .WhereIf(!input.Email.IsNullOrWhiteSpace(), n => n.Email.Contains(input.Email))
    .ApplyCustomFilters(input.CustomFilters)
    .OrderBy(input.Sorting);

var count = await qry.CountAsync();
var pagedResult = await qry.PageBy(input).ToListAsync();
```

`ApplyCustomFilters` 根据字段名、操作符和值动态构建 LINQ 表达式，支持嵌套属性和常见操作符：

```csharp
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

### 事件驱动与异步消息处理

当前项目存在事件驱动设计，主要由 **RabbitMQ + Rebus** 承载异步消息链路。典型场景是渠道客户、渠道账户创建流程：业务接口先落库请求记录，事务提交后发布消息，由 Callback Host 消费并执行后续步骤。

```text
业务接口
-> 写入 PayChannelServiceRequest
-> UnitOfWork 提交完成
-> Publish Rebus 消息
-> RabbitMQ
-> Callback Host 消费
-> IHandleMessages<T> Handler
```

Rebus 消费端配置：

```csharp
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

```csharp
var bus = app.ApplicationServices.GetRequiredService<IBus>();
bus.Subscribe<CreatePayChannelAccountEto>().Wait();
```

事务提交后发布消息：

```csharp
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

消息对象：

```csharp
public class CreatePayChannelCustomerEto : EventData
{
    public int UserId { get; set; }
    public Guid KycApplicantId { get; set; }
    public Guid? RequestId { get; set; }
}

public class CreatePayChannelAccountEto : EventData
{
    public Guid RequestId { get; set; }
}
```

消费者 Handler：

```csharp
public class CreatePayChannelCustomerHandler :
    IHandleMessages<CreatePayChannelCustomerEto>,
    ITransientDependency
{
    [UnitOfWork]
    public virtual async Task Handle(CreatePayChannelCustomerEto message)
    {
        // 创建渠道客户，成功后继续触发账户创建
        await _bus.Publish(new CreatePayChannelAccountEto
        {
            RequestId = message.RequestId.Value
        });
    }
}
```

```csharp
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

项目中也保留了 ABP EventBus 本地事件模型：

```csharp
public class KycVerificationCompletedEvent : EventData
{
    public int UserId { get; set; }
    public Guid KycApplicantId { get; set; }
    public bool IsApproved { get; set; }
}
```

```csharp
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

说明：当前主要生效的是 RabbitMQ/Rebus 消息驱动链路；KYC 的 ABP EventBus 触发点保留在代码中，但触发调用处目前是注释状态。

## 构建

```bash
dotnet build ClientPlatform.sln
```
