# 项目核心约束

> 本文件记录项目的**不可变约束**。
> 所有 Agent 必须遵守，违反等同于需求变更，需走 ADR 流程。

## 技术栈

| 层 | 技术 | 说明 |
|----|------|------|
| 运行时 | .NET Core 8 | LTS 版本，纯后端 API 项目，无前端 |
| Web 框架 | ASP.NET Core Controller | Controller 模式，路由声明式 |
| **ORM** | **FreeSql** | 支持多数据库切换（SQLite / PostgreSQL / Oracle 等） |
| 数据库 | **可配置** | 通过 `appsettings.json` 切换，默认 SQLite |
| 映射 | AutoMapper | DTO ↔ Entity 转换 |
| JSON 序列化 | System.Text.Json | .NET 内置高性能序列化 |
| xlsx 生成 | ClosedXML | 轻量级 xlsx 文件生成库（基于 OpenXML） |
| 输入校验 | FluentValidation | DTO 请求体校验 |
| **认证** | **JWT Bearer Token** | API 调用鉴权 |
| 日志 | **Serilog** + 文件滚动 + 数据库表 | 双通道日志记录 |
| 测试 | xUnit + Moq | 单元测试 + Mock |
| 集成测试 | Microsoft.AspNetCore.Mvc.Testing | WebApplicationFactory |
| 文档 | Swagger (Swashbuckle) | 自动生成 OpenAPI 文档，支持 JWT 配置 |

## 系统架构

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        第三方系统 (Third-Party System)                     │
│   1. POST /api/v1/auth/token          → 获取 JWT Token                  │
│   2. POST /api/v1/projects            → 创建项目                        │
│   3. POST /api/v1/projects/{id}/piles → 添加基桩                        │
│   4. POST /api/v1/piles/{id}/...      → 添加剖面统计/测点数据/报告        │
│   5. GET  .../export                  → 导出 xlsx                       │
└───────────────────┬─────────────────────────────────────────────────────┘
                    │ JSON + Bearer Token
                    ↓
┌─────────────────────────────────────────────────────────────────────────┐
│              Web API (.NET Core 8)                                       │
│                                                                         │
│  ┌─────────────────────┐   ┌───────────────────────┐                   │
│  │  JWT Middleware     │   │  ExceptionHandlingMiddleware               │
│  │  (鉴权/授权)        │   │  (统一错误处理)                            │
│  └─────────┬───────────┘   └───────────────────────┘                   │
│            ↓                                                           │
│  ┌─────────────────────┐   ┌───────────────────────┐                   │
│  │   Controller        │→  │   Service             │                   │
│  │   (DTO校验)         │   │   (业务逻辑)          │                   │
│  └─────────────────────┘   └───────────┬───────────┘                   │
│                                        ↓                               │
│  ┌─────────────────────┐   ┌───────────────────────┐                   │
│  │   FreeSql Repository│  │   Serilog Logger       │                   │
│  │   (多DB切换)        │   │   (文件+DB双写)        │                   │
│  └──────────┬──────────┘   └───────────────────────┘                   │
│             ↓                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                    Database (可切换)                              │  │
│  │  ┌───────────────┐  ┌───────────────┐  ┌──────────────────────┐ │  │
│  │  │   project_info │←│   pile_info   │→│ measurement_data     │ │  │
│  │  └───────────────┘  └───────────────┘  └──────────────────────┘ │  │
│  │                     ┌───────────────┐  ┌──────────────────────┐ │  │
│  │                     │ profile_stat  │  │   pile_report       │ │  │
│  │                     └───────────────┘  └──────────────────────┘ │  │
│  │  ┌───────────────┐  ┌────────────────────────────────────────┐ │  │
│  │  │ project_report│  │        api_log (日志表)                 │ │  │
│  │  └───────────────┘  └────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

### 性能目标

- 单条桩信息写入：< 200ms
- 批量测量数据写入（1000 条）：< 2 秒
- 单桩聚合查询（含所有剖面和测点）：< 500ms
- 分页列表查询（100 条/页）：< 300ms
- 单桩 xlsx 导出（120 测点）：< 3 秒
- 项目 xlsx 导出（20 根桩）：< 10 秒

## 强制要求

### 1. 测试全覆盖
所有业务功能必须有对应测试用例：
- Service 层每个公共方法至少一个正常路径 + 一个异常路径测试
- Controller 层使用 WebApplicationFactory 进行集成测试
- JSON 请求体校验测试覆盖：完整数据、字段缺失、字段格式错误、边界值
- xlsx 导出测试覆盖：正常导出、无数据导出
- JWT 鉴权测试覆盖：无 token、token 过期、无效 token、有效 token

### 2. 版本化 API
- 所有 API 路由以 `/api/v1/` 前缀开头
- 如需版本升级，新增 `/api/v2/`，保留旧版本兼容
- 路由版本号在 `appsettings.json` 中配置

### 3. 数据字典
- 数据结构变更必须在 `doc/data-dictionary/` 记录
- 新增数据字段时标明版本号和变更日期
- 字段定义包含：中文名、英文名、类型、长度、精度、说明、示例、对应 xlsx 列名

### 4. Hermes 合规
所有 Agent 交互严格遵循 `agents.md` 定义的职责边界和协作协议

### 5. JSON 请求规范

| 要求 | 说明 |
|------|------|
| 编码 | UTF-8 |
| Content-Type | `application/json` |
| 数值格式 | 使用 `.` 作为小数分隔符，不包含千分位分隔符 |
| 时间格式 | ISO 8601 UTC（如 `2025-09-23T00:00:00Z`） |
| 空值处理 | 可选字段为 NULL 时省略或传 `null` |
| 数组 | 批量操作时使用 JSON 数组 `[...]` |
| 鉴权头 | `Authorization: Bearer <token>` |

### 6. 配置管理

- 应用配置通过 `appsettings.json` + 环境变量覆盖
- 数据库配置支持多数据库自由切换
- xlsx 导出模板路径、导出文件名格式可配置
- JWT Secret 从环境变量读取，不硬编码
- 日志级别和滚动策略可配置

**appsettings.json 示例：**

```json
{
  "Database": {
    "Provider": "Sqlite",
    "ConnectionString": "Data Source=dbdata/pile.db"
  },
  "Jwt": {
    "Issuer": "PileDetectionApi",
    "Audience": "PileDetectionClient",
    "ExpireMinutes": 1440
    // Secret 从环境变量 JWT_SECRET 读取
  },
  "Logging": {
    "File": {
      "Path": "Logs/pile-{Date}.log",
      "RetentionDays": 30
    },
    "Table": true
  }
}
```

**数据库切换示例：**

```json
// SQLite 模式
{ "Provider": "Sqlite", "ConnectionString": "Data Source=Data/pile.db" }

// PostgreSQL 模式
{ "Provider": "PostgreSQL", "ConnectionString": "Host=localhost;Port=5432;Database=pile;Username=user;Password=pwd" }

// Oracle 模式
{ "Provider": "Oracle", "ConnectionString": "User Id=user;Password=pwd;Data Source=//localhost:1521/pile" }
```

### 7. 安全要求

| 要求 | 实现方式 |
|------|----------|
| **API 认证** | **JWT Bearer Token**，所有业务接口需携带有效 token |
| Token 签发 | POST `/api/v1/auth/token` 返回 token（预共享密钥或 client_credentials） |
| 输入验证 | FluentValidation 校验 JSON 请求体所有字段 |
| 超时保护 | 异步操作统一使用 CancellationToken，默认超时 30 秒 |
| SQL 注入 | FreeSql 参数化查询，禁止原生 SQL 拼接 |
| 请求大小限制 | JSON 请求体限制 ≤ 10MB（`[RequestSizeLimit]`） |

### 8. FreeSql ORM 规范

| 要求 | 实现方式 |
|------|----------|
| 数据实体 | 使用 FreeSql `[Table]` 和 `[Column]` 特性标注 |
| 自动建表 | FreeSql CodeFirst 自动迁移，`SyncStructure` 同步实体结构 |
| 多数据库 | 通过 FreeSqlBuilder 根据配置动态选择 Provider |
| **表名大小写** | FreeSql `NameConvertType` 配置化处理不同数据库兼容 |
| **列名大小写** | 实体属性统一 PascalCase，表配置决定最终存储格式 |
| 仓储模式 | 统一使用 `BaseRepository<T>` 或 `IFreeSql` 的 `Select/Insert/Update/Delete` |
| 事务 | 批量操作使用 `IUnitOfWork` 或 `Repository.UnitOfWork` |
| 软删除 | FreeSql 内置 `[Column(IsVersion = true)]` 或自定义 IsDeleted 字段 |
| 导航属性 | 使用 `Navigate` 特性定义关联关系 |

**FreeSql 名转换配置示例：**

```csharp
// 根据数据库类型自动选择命名策略
fsql = new FreeSqlBuilder()
    .UseConnectionString(dbType, connectionString)
    .UseNameConvert(NameConvertType.PascalCaseToUnderscore) // 蛇形命名
    .Build();
```

### 9. 日志规范

| 要求 | 实现方式 |
|------|----------|
| 日志框架 | Serilog，双通道输出 |
| **日志文件** | 滚动日志文件，按日期切割，保留 30 天 |
| **日志表** | API 调用日志写入 `api_log` 表 |
| 日志级别 | Information / Warning / Error / Fatal |
| 请求日志 | 自动记录：请求路径、方法、状态码、耗时、客户端 IP |
| 审计日志 | 记录数据创建/更新操作者信息和操作时间 |

**api_log 表结构：**

| 字段 | 类型 | 说明 |
|------|------|------|
| id | bigint | 主键自增 |
| endpoint | varchar(500) | 请求路径 |
| http_method | varchar(10) | HTTP 方法 |
| request_body | text | 请求体（脱敏后） |
| response_code | int | 响应状态码 |
| response_body | text | 响应体（可选记录） |
| client_ip | varchar(50) | 客户端 IP |
| duration_ms | int | 请求耗时（毫秒） |
| created_at | datetime | 记录时间 UTC |

### 10. 数据库规范

| 要求 | 实现方式 |
|------|----------|
| ORM | 统一使用 FreeSql，CodeFirst 自动迁移 |
| 数据库切换 | 配置驱动，支持 SQLite / PostgreSQL / Oracle / MySQL |
| 表命名 | 统一使用全小写+下划线（snake_case） |
| 列命名 | 统一使用全小写+下划线（snake_case） |
| 时间字段 | 统一使用 UTC 时间存储 |
| 级联删除 | 父级删除时级联删除关联子表数据 |
| 事务 | 批量写入使用数据库事务，单次提交保证原子性 |
| 模型版本 | 每条数据记录存储 api_version |

### 11. 删除规范

- 基桩数据和项目数据不做物理删除，通过 `is_deleted` 软删除标记
- 日志文件按时间滚动删除，保留最近 30 天
- 日志表数据定期清理，保留最近 90 天
- 数据库迁移不做逆向回滚（仅向前迁移）

## 数据模型完整关系

```
┌─────────────────┐       ┌──────────────────────────────────┐
│   project_info  │       │          pile_info               │
├─────────────────┤       ├──────────────────────────────────┤
│ id (PK)         │←──────│ id (PK)                          │
│ project_name    │ 1:N   │ project_id (FK) → project_info   │
│ project_no      │       │ pile_name                        │
│ project_location│       │ design_length                    │
│ project_manager │       │ design_diameter                  │
│ project_desc    │       │ design_strength                  │
│ is_deleted      │       │ pour_date                        │
│ created_at      │       │ test_date                        │
│ updated_at      │       │ test_standard                    │
│ api_version     │       │ instrument_model                 │
└─────────────────┘       │ instrument_sn                    │
        │                  │ certification_no                 │
        │ 1:1             │ tester                           │
        ↓                 │ tester_cert_no                   │
┌─────────────────┐       │ integrity_category (Ⅰ/Ⅱ/Ⅲ/Ⅳ)    │
│ project_report  │       │ is_deleted                       │
├─────────────────┤       │ created_at                       │
│ id (PK)         │       │ updated_at                       │
│ project_id (FK) │       │ api_version                      │
│ report_no       │       └────────┬─────────────────────────┘
│ report_date     │                │
│ conclusion      │         1:N    ├──────────────────────┐
│ integrity_summary│               │                      │
│ created_at      │               ↓                      ↓
│ updated_at      │  ┌─────────────────────┐  ┌─────────────────────┐
└─────────────────┘  │  measurement_data   │  │   profile_stat      │
                     ├─────────────────────┤  ├─────────────────────┤
      1:1            │ id (PK)             │  │ id (PK)             │
       ↑             │ pile_info_id (FK)   │  │ pile_info_id (FK)   │
┌─────────────────┐  │ profile (剖面标识)  │  │ profile (剖面标识)   │← UNIQUE
│   pile_report   │  │ depth (深度)        │  │ distance (测距mm)   │
├─────────────────┤  │ sound_velocity(波速)│  │ max_velocity        │
│ id (PK)         │  │ amplitude (幅度)    │  │ min_velocity        │
│ pile_info_id(FK)│  │ sound_time (声时)   │  │ avg_velocity        │
│ report_no       │  │ psd (PSD值)        │  │ std_velocity        │
│ report_date     │  │ created_at         │  │ cv_velocity (离差)  │
│ integrity_category│ │ updated_at         │  │ critical_velocity   │
│ avg_velocity    │  │ api_version        │  │ max_amplitude       │
│ critical_velocity│ └─────────────────────┘  │ min_amplitude       │
│ avg_amplitude   │                          │ avg_amplitude       │
│ critical_amplitude│                         │ std_amplitude       │
│ conclusion      │                          │ cv_amplitude(离差)  │
│ created_at      │                          │ critical_amplitude  │
│ updated_at      │                          │ created_at          │
└─────────────────┘                          │ updated_at          │
                                             │ api_version         │
┌──────────────────────────────────────────┐ └─────────────────────┘
│              api_log                     │
├──────────────────────────────────────────┤
│ id (PK)                                  │
│ endpoint                                 │
│ http_method                              │
│ request_body                             │
│ response_code                            │
│ response_body                            │
│ client_ip                                │
│ duration_ms                              │
│ created_at                               │
└──────────────────────────────────────────┘
```

## API 端点定义

### 认证管理

| 方法 | 路由 | 说明 | 鉴权 |
|------|------|------|------|
| POST | `/api/v1/auth/token` | 获取 JWT Token | ❌ 无需鉴权 |

### 项目信息管理

| 方法 | 路由 | 说明 | 鉴权 |
|------|------|------|------|
| POST | `/api/v1/projects` | 创建项目 | ✅ |
| GET | `/api/v1/projects` | 分页查询项目列表 | ✅ |
| GET | `/api/v1/projects/{id}` | 查询项目详情（含桩数量统计） | ✅ |
| PUT | `/api/v1/projects/{id}` | 更新项目信息 | ✅ |
| DELETE | `/api/v1/projects/{id}` | 软删除项目 | ✅ |
| GET | `/api/v1/projects/{id}/export` | 导出项目 xlsx（汇总所有桩） | ✅ |

### 基桩信息管理

| 方法 | 路由 | 说明 | 鉴权 |
|------|------|------|------|
| POST | `/api/v1/projects/{projectId}/piles` | 在项目下创建基桩 | ✅ |
| GET | `/api/v1/projects/{projectId}/piles` | 查询项目下所有基桩列表 | ✅ |
| GET | `/api/v1/piles/{id}` | 查询单桩完整信息 | ✅ |
| PUT | `/api/v1/piles/{id}` | 更新基桩信息 | ✅ |
| DELETE | `/api/v1/piles/{id}` | 软删除基桩 | ✅ |
| GET | `/api/v1/piles/{id}/export` | 导出单桩 xlsx 报告 | ✅ |

### 剖面统计数据管理

| 方法 | 路由 | 说明 | 鉴权 |
|------|------|------|------|
| POST | `/api/v1/piles/{pileId}/profiles` | 批量添加剖面统计 | ✅ |
| GET | `/api/v1/piles/{pileId}/profiles` | 查询桩的所有剖面统计 | ✅ |
| PUT | `/api/v1/piles/{pileId}/profiles/{id}` | 更新剖面统计 | ✅ |
| DELETE | `/api/v1/piles/{pileId}/profiles/{id}` | 删除剖面统计 | ✅ |

### 测点数据管理

| 方法 | 路由 | 说明 | 鉴权 |
|------|------|------|------|
| POST | `/api/v1/piles/{pileId}/measurements` | 批量添加测点数据 | ✅ |
| GET | `/api/v1/piles/{pileId}/measurements` | 查询所有测点（支持深度范围筛选） | ✅ |
| GET | `/api/v1/piles/{pileId}/measurements/profile/{profile}` | 按剖面查询测点 | ✅ |
| PUT | `/api/v1/piles/{pileId}/measurements/{id}` | 更新单条测点数据 | ✅ |
| DELETE | `/api/v1/piles/{pileId}/measurements/{id}` | 删除单条测点数据 | ✅ |

### 报告管理

| 方法 | 路由 | 说明 | 鉴权 |
|------|------|------|------|
| POST | `/api/v1/piles/{pileId}/report` | 创建/更新单桩报告 | ✅ |
| GET | `/api/v1/piles/{pileId}/report` | 查询单桩报告 | ✅ |
| POST | `/api/v1/projects/{projectId}/report` | 创建/更新项目报告 | ✅ |
| GET | `/api/v1/projects/{projectId}/report` | 查询项目报告 | ✅ |

## JSON 请求体结构

### POST /api/v1/auth/token — 获取 Token

为简化系统间认证，使用预共享 API Key 换取 JWT Token。

```json
{
    "apiKey": "pile-detection-secret-key-2026",
    "clientId": "third-party-system-a"
}
```

```json
// 响应
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

### POST /api/v1/projects — 创建项目

```json
{
    "projectName": "碗窑岭大桥",
    "projectNo": "WYL-2025-001",
    "projectLocation": "浙江省温州市",
    "projectManager": "李四",
    "projectDesc": "碗窑岭大桥基桩检测项目"
}
```

### POST /api/v1/projects/{projectId}/piles — 创建基桩

```json
{
    "pileName": "右11",
    "designLength": 11.700,
    "designDiameter": 2000,
    "designStrength": "C25",
    "pourDate": "2025-09-23T00:00:00Z",
    "testDate": "2025-09-23T00:00:00Z",
    "testStandard": "JGJ 106-2014",
    "instrumentModel": "U5700",
    "instrumentSn": "U51811005",
    "certificationNo": "Cert-2025-001",
    "tester": "张三",
    "testerCertNo": "JC-2025-088"
}
```

### POST /api/v1/piles/{pileId}/profiles — 批量添加剖面统计

```json
[
    {
        "profile": "1-2",
        "distance": 1022,
        "maxVelocity": 4.211,
        "minVelocity": 1.042,
        "avgVelocity": 4.026,
        "stdVelocity": 0.0947,
        "cvVelocity": 0.0235,
        "criticalVelocity": 3.805,
        "maxAmplitude": 135.24,
        "minAmplitude": 107.07,
        "avgAmplitude": 128.979,
        "stdAmplitude": 7.128,
        "cvAmplitude": 0.055,
        "criticalAmplitude": 122.979
    }
]
```

### POST /api/v1/piles/{pileId}/measurements — 批量添加测点

```json
[
    {
        "depth": 0.00,
        "profile": "1-2",
        "soundVelocity": 1.042,
        "amplitude": 110.98,
        "soundTime": 981.07,
        "psd": 0.000
    }
]
```

### POST /api/v1/piles/{pileId}/report — 创建单桩报告

```json
{
    "reportNo": "BG-2025-001",
    "reportDate": "2025-09-23T00:00:00Z",
    "integrityCategory": 1,
    "avgVelocity": 4.026,
    "criticalVelocity": 3.805,
    "avgAmplitude": 128.979,
    "criticalAmplitude": 122.979,
    "conclusion": "该桩混凝土均匀、密实、桩身完整，完整性类别为Ⅰ类桩。"
}
```

### POST /api/v1/projects/{projectId}/report — 创建项目报告

```json
{
    "reportNo": "PRJ-2025-001",
    "reportDate": "2025-09-25T00:00:00Z",
    "conclusion": "所有基桩检测结果均达到Ⅰ类桩标准。",
    "integritySummary": "Ⅰ类桩: 10根, Ⅱ类桩: 0根, Ⅲ类桩: 0根, Ⅳ类桩: 0根"
}
```

## xlsx 导出格式

### 单桩导出（`/api/v1/piles/{id}/export`）

严格参照 `doc/需求/pile1.xlsx`，包含 3 个 Sheet：

- **Sheet 1「桩信息」** — 当前桩的基本参数
- **Sheet 2「数据表」** — 所有剖面的统计数据和原始测点数据
- **Sheet 3「单桩报告」** — 完整性类别判定结果

### 项目导出（`/api/v1/projects/{id}/export`）

包含所有桩的汇总数据，额外附加：

- **Sheet 1「项目概况」** — 项目基本信息 + 所有桩列表
- **Sheet 2「各桩统计」** — 所有桩的关键统计指标汇总表
- **Sheet 3「单桩报告」** — 各桩的逐桩详细报告（每桩一页）

## 数据流方向

```
认证流程:
  第三方系统 → POST /api/v1/auth/token [JSON]
             → 验证 apiKey
             → 签发 JWT Token (含 clientId, 有效期)
             → 返回 token

写入流程（整体事务顺序）:
  1. POST /api/v1/projects [Bearer Token] → 创建项目，获取 projectId
  2. POST /api/v1/projects/{projectId}/piles [Bearer Token] → 创建基桩，获取 pileId
  3. POST /api/v1/piles/{pileId}/profiles [Bearer Token] → 添加剖面统计
  4. POST /api/v1/piles/{pileId}/measurements [Bearer Token] → 批量添加测点
  5. POST /api/v1/piles/{pileId}/report [Bearer Token] → 创建单桩报告
  6. POST /api/v1/projects/{projectId}/report [Bearer Token] → 创建项目报告

查询流程:
  第三方系统 → GET [Bearer Token] → Controller → Service → FreeSql Select
             → 返回 JSON

导出流程:
  第三方系统 → GET .../export [Bearer Token] → ExportService
             → FreeSql 查询 DB → ClosedXML 生成 xlsx
             → 返回文件流下载

日志流程:
  每次请求 → Middleware 拦截 → 记录请求/响应信息
          → Serilog 写入日志文件 + api_log 表
```

## 关键索引

| 表 | 索引列 | 说明 |
|----|--------|------|
| project_info | project_name | 项目名称查询加速 |
| project_info | project_no | 项目编号唯一查询 |
| pile_info | project_id + pile_name | 项目下桩名唯一约束 |
| pile_info | pile_name | 桩名查询 |
| measurement_data | pile_info_id + profile + depth | 复合索引：按桩+剖面+深度查询 |
| measurement_data | pile_info_id + depth | 深度范围筛选 |
| profile_stat | pile_info_id + profile | 唯一剖面约束 |
| api_log | created_at | 日志按时间清理 |
