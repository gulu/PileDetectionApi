# 设计哲学与编码规范

> 本文件定义项目的"灵魂"——价值观、风格、规范。
> 所有代码必须符合这里的约定。当规范与其他文件冲突时，以此为准。

## 一、核心原则

1. **简洁优先** — 代码应当简洁明了，避免过度设计。一个函数只做一件事。
2. **可测试性** — 所有模块应当易于测试。如果一个函数难以测试，说明设计有问题。
3. **可维护性** — 代码结构清晰，命名自文档化，便于维护和扩展。
4. **一致性** — 同类问题同类解法，不引入不必要的风格变化。
5. **容错设计** — 单条数据校验失败不影响整体导入，优先返回处理结果报告。
6. **安全第一** — 所有 API 接口（除认证外）必须鉴权，数据访问必须授权。
7. **数据库无关** — 业务逻辑层不感知底层数据库类型，通过 FreeSql 统一抽象。

## 二、架构原则

```
┌─────────────────────────────────────────────────────────────────────┐
│                       第三方系统                                     │
│          JSON + Bearer Token → API → JSON Response                  │
└────────────────────────────┬────────────────────────────────────────┘
                             ↓
┌─────────────────────────────────────────────────────────────────────┐
│                     ASP.NET Core Middleware Pipeline                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────────────────┐  │
│  │Exception │→│ Serilog  │→│   JWT    │→│    Controller      │  │
│  │Handling  │  │ Logging  │  │  Auth    │  │  (DTO校验+路由)    │  │
│  └──────────┘  └──────────┘  └──────────┘  └─────────┬──────────┘  │
│                                                       ↓             │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Service 层                                  │   │
│  │  PileService / ProjectService / ReportService / ExportService │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐  │   │
│  │  │  IValidator  │  │  AutoMapper  │  │  FreeSql (IFreeSql)│  │   │
│  │  └──────────────┘  └──────────────┘  └─────────┬──────────┘  │   │
│  └────────────────────────────────────────────────┼──────────────┘   │
│                                                    ↓                 │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                      Database                                  │   │
│  │  SQLite / PostgreSQL / Oracle / MySQL (配置切换)               │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

**数据流：**
```
写入方向（第三方系统 → 本系统）：
  Third-Party → POST/PUT JSON + Bearer Token
    → JWT Middleware 验证 token
    → Controller 接收并校验 DTO
    → Service 业务逻辑处理
    → FreeSql Repository 写入 DB
    → Serilog 记录操作日志（文件+表）
    → 返回统一 ApiResponse

查询方向：
  Third-Party → GET + Bearer Token
    → JWT Middleware 验证 token
    → Controller → Service → FreeSql Select
    → DTO 返回 JSON

导出方向：
  Third-Party → GET .../export + Bearer Token
    → ExportService
    → FreeSql 查询 → ClosedXML 生成 xlsx
    → 文件流下载
```

## 三、命名规范

### 3.1 C# 命名

| 类别 | 规范 | 示例 |
|------|------|------|
| 命名空间 | PascalCase | `PileApi.Controllers`, `PileApi.Services` |
| 类 | PascalCase | `PileController`, `PileService`, `PileInfoEntity` |
| 接口 | I + PascalCase | `IPileService`, `IPileRepository` |
| 方法 | PascalCase | `CreatePileAsync()`, `GetPileByIdAsync()` |
| 属性 | PascalCase | `ProjectName`, `PileName` |
| 字段（私有） | _camelCase | `_pileRepository`, `_logger` |
| 参数 | camelCase | `pileId`, `createDto` |
| 局部变量 | camelCase | `pileInfo`, `totalCount` |
| 常量 | PascalCase | `DefaultPageSize`, `MaxExportRows` |
| 枚举 | PascalCase | `PileCategory`, `IntegrityClass` |

### 3.2 数据库命名（FreeSql 兼容）

| 类别 | 规范 | 示例 | 说明 |
|------|------|------|------|
| 表名 | snake_case | `pile_info` | 统一蛇形命名，FreeSql `PascalCaseToUnderscore` |
| 列名 | snake_case | `project_name` | 同上，跨数据库兼容 |
| 主键 | id | `id` | 所有表统一使用 `id` 作为主键 |
| 外键 | 表名单数_id | `pile_info_id` | 如 `pile_info_id` |
| 索引 | idx_表名_列名 | `idx_pile_info_project_id` | 统一前缀 `idx_` |

**数据库大小写策略：**

| 数据库类型 | 实际存储 | FreeSql 配置 |
|-----------|---------|-------------|
| SQLite | 不区分大小写 | `PascalCaseToUnderscore` |
| PostgreSQL | 全小写 | `PascalCaseToUnderscore` |
| Oracle | 全大写 | `PascalCaseToUpper`（特殊处理） |
| MySQL | 依 OS 配置 | `PascalCaseToUnderscore` + `lower_case_table_names=1` |

### 3.3 API 路由命名

| 类别 | 规范 | 示例 |
|------|------|------|
| 资源路由 | 小写 + 复数 | `/api/v1/projects` |
| 子资源 | 父/父id/子 | `/api/v1/projects/{projectId}/piles` |
| 单资源 | 资源/{id} | `/api/v1/piles/{id}` |
| 动作路由 | 资源/{id}/操作 | `/api/v1/piles/{id}/export` |

### 3.4 JSON 请求/响应字段命名

| 类别 | 规范 | 示例 |
|------|------|------|
| JSON 字段 | camelCase | `projectName`, `pileName` |
| 数组字段 | 复数名词 | `measurements`, `profiles` |
| 时间字段 | ISO 8601 | `2025-09-23T00:00:00Z` |
| 枚举字段 | 数字 | `integrityCategory: 1`（对应Ⅰ类） |
| 鉴权头 | Authorization | `Bearer eyJhbGci...` |

### 3.5 配置文件

| 类别 | 格式 | 示例 |
|------|------|------|
| 应用配置 | JSON | `appsettings.json` |
| 环境配置 | JSON | `appsettings.Development.json` |
| 数据字典 | Markdown | `数据字典-V1.0.md` |
| API 文档 | Swagger | 自动生成 OpenAPI 文档 |

## 四、代码风格

### 4.1 C# 代码风格

- 使用 `var` 当类型从右侧显式可知
- 异步方法统一以 `Async` 后缀结尾
- 使用 C# 12 特性（主构造函数、集合表达式等）
- 文件级命名空间（`namespace X.Y;`）
- 大括号使用 Allman 风格
- 每行最长 120 字符
- 使用 `record` 定义 DTO，`class` 定义实体和服务
- 私有方法置于类底部，按调用层级排列

### 4.2 项目结构

```
PileDetectionApi/
├── Controllers/              # API 控制器
│   ├── AuthController.cs     # Token 签发
│   ├── ProjectController.cs  # 项目管理
│   ├── PileController.cs     # 基桩管理
│   ├── ProfileController.cs  # 剖面统计
│   ├── MeasurementController.cs  # 测点数据
│   └── ReportController.cs   # 报告管理
│
├── Services/                 # 业务逻辑层
│   ├── Interfaces/
│   │   ├── IProjectService.cs
│   │   ├── IPileService.cs
│   │   ├── IProfileStatService.cs
│   │   ├── IMeasurementService.cs
│   │   ├── IReportService.cs
│   │   ├── IExportService.cs
│   │   └── IAuthService.cs
│   ├── ProjectService.cs
│   ├── PileService.cs
│   ├── ProfileStatService.cs
│   ├── MeasurementService.cs
│   ├── ReportService.cs
│   ├── ExportService.cs
│   └── AuthService.cs
│
├── Entities/                 # FreeSql 实体模型
│   ├── ProjectInfoEntity.cs
│   ├── PileInfoEntity.cs
│   ├── ProfileStatEntity.cs
│   ├── MeasurementDataEntity.cs
│   ├── PileReportEntity.cs
│   ├── ProjectReportEntity.cs
│   └── ApiLogEntity.cs
│
├── DTOs/                     # 数据传输对象
│   ├── Request/
│   │   ├── AuthTokenRequest.cs
│   │   ├── CreateProjectRequest.cs
│   │   ├── CreatePileRequest.cs
│   │   ├── CreateProfileStatRequest.cs
│   │   ├── CreateMeasurementRequest.cs
│   │   ├── CreatePileReportRequest.cs
│   │   └── CreateProjectReportRequest.cs
│   └── Response/
│       ├── ApiResponse.cs
│       ├── PagedResponse.cs
│       ├── AuthTokenResponse.cs
│       ├── ProjectResponse.cs
│       ├── PileResponse.cs
│       └── ...
│
├── Mappings/                 # AutoMapper 配置
│   └── MappingProfile.cs
│
├── Data/                     # FreeSql 配置
│   └── FreeSqlSetup.cs       # FreeSqlBuilder 工厂
│
├── Validators/               # FluentValidation 验证器
│   ├── CreateProjectRequestValidator.cs
│   ├── CreatePileRequestValidator.cs
│   └── CreateMeasurementRequestValidator.cs
│
├── Middleware/               # 中间件
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs  # API 调用日志
│   └── ...
│
├── Extensions/               # 扩展方法
│   └── ServiceCollectionExtensions.cs
│
├── Configs/                  # 强类型配置
│   ├── DatabaseConfig.cs
│   ├── JwtConfig.cs
│   └── LoggingConfig.cs
│
└── Program.cs
```

### 4.3 导入规范

```csharp
// 系统命名空间
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 第三方包
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using FluentValidation;
using ClosedXML.Excel;

// 项目内部命名空间
using PileDetectionApi.Entities;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;
```

### 4.4 FreeSql 实体示例

```csharp
using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

[Table(Name = "project_info")]
public class ProjectInfoEntity
{
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    [Column(Name = "project_name", DbType = "varchar(200)")]
    public string ProjectName { get; set; } = string.Empty;

    [Column(Name = "project_no", DbType = "varchar(100)", IsNullable = true)]
    public string? ProjectNo { get; set; }

    [Column(Name = "is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column(Name = "created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(Name = "updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column(Name = "api_version", DbType = "varchar(20)")]
    public string ApiVersion { get; set; } = "v1";

    // 导航属性
    [Navigate(nameof(PileInfoEntity.ProjectId))]
    public List<PileInfoEntity> Piles { get; set; } = new();
}
```

## 五、错误处理

### 5.1 全局异常中间件

```csharp
public class ExceptionHandlingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await WriteErrorResponse(context, "数据校验失败", ex.Message);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await WriteErrorResponse(context, "资源不存在", ex.Message);
        }
        catch (DuplicateException ex)
        {
            context.Response.StatusCode = 409;
            await WriteErrorResponse(context, "数据重复", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = 401;
            await WriteErrorResponse(context, "未授权", ex.Message);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            await WriteErrorResponse(context, "服务器内部错误", "请联系管理员");
        }
    }
}
```

### 5.2 Service 层

- 业务异常使用自定义异常类（`NotFoundException`, `DuplicateException`, `DataValidationException`, `AuthException`）
- FreeSql 操作异常捕获后包装为 `DataAccessException`
- Service 层不做 HTTP 状态码返回，统一抛异常由中间件处理

### 5.3 API 层

- Controller 层尽量无业务逻辑，只做参数校验和路由
- 使用 `[ApiController]` + `[Authorize]` 特性
- 使用 `[FromBody]` 显式标注 JSON 请求体

## 六、日志规范

- 使用 **Serilog** 作为日志框架，双通道输出
- **文件日志**：按日期滚动切割，保留 30 天
  - 路径：`Logs/pile-{Date}.log`
  - 格式：`{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}`
- **数据库日志**：API 调用写入 `api_log` 表
  - 通过 `RequestLoggingMiddleware` 自动记录
  - 包含：endpoint, http_method, request_body, response_code, client_ip, duration_ms
- 日志级别：
  - `Information` — 接口调用开始/结束（含耗时）、数据创建/更新记录
  - `Warning` — 数据校验告警（不阻断）、调用超时（>3秒）
  - `Error` — 数据库写入失败、未处理异常、xlsx 生成失败
- 敏感信息脱敏（Token、密码等不记录到日志）

## 七、API 响应格式

### 成功响应（单条）

```json
{
    "code": 200,
    "message": "success",
    "data": {
        "id": 1,
        "projectName": "碗窑岭大桥",
        "pileName": "右11"
    },
    "timestamp": "2026-05-11T06:16:21Z"
}
```

### 成功响应（分页列表）

```json
{
    "code": 200,
    "message": "success",
    "data": {
        "items": [...],
        "page": 1,
        "pageSize": 20,
        "totalCount": 150,
        "totalPages": 8
    },
    "timestamp": "2026-05-11T06:16:21Z"
}
```

### 创建成功响应

```json
{
    "code": 201,
    "message": "创建成功",
    "data": { "id": 1 },
    "timestamp": "2026-05-11T06:16:21Z"
}
```

### Token 响应

```json
{
    "code": 200,
    "message": "success",
    "data": {
        "token": "eyJhbGciOiJIUzI1NiIs...",
        "expiresAt": "2026-05-12T06:16:21Z",
        "tokenType": "Bearer"
    },
    "timestamp": "2026-05-11T06:16:21Z"
}
```

### 错误响应

```json
{
    "code": 401,
    "message": "未授权",
    "errors": { "token": "Token 已过期或无效" },
    "timestamp": "2026-05-11T06:16:21Z"
}
```

## 八、测试规范

- 使用 xUnit + Moq 进行单元测试
- 测试项目命名：`PileDetectionApi.Tests`
- 测试类命名：`{被测类}Tests`
- 测试方法命名：`{方法}_{场景}_Should{预期}`
- 分层覆盖：
  - **单元测试**：Service 层逻辑，Mock FreeSql
  - **集成测试**：Controller 层，使用 WebApplicationFactory + 测试数据库
  - **鉴权测试**：无 token/过期 token/无效 token 场景
- 覆盖率目标：Service 层 ≥ 90%，整体 ≥ 80%
- JSON 校验测试覆盖：完整数据、字段缺失、字段格式错误、边界值
- 导出测试覆盖：正常导出、无数据导出、大数据量导出
- 日志测试覆盖：日志写入、日志文件滚动
