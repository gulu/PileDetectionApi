# Agent 职责边界与协作协议

> Hermes Agent 严格遵守此文件定义的职责边界。
> 每个 Agent 实例负责其角色范围内的任务，
> 对于跨角色任务，必须严格遵循协议交接。

## 一、角色定义

### 1. Orchestrator（编排者）— 默认的 YOU

| 属性 | 值 |
|------|-----|
| 角色名 | `orchestrator` |
| 职责 | 规划、拆解、分配、审核、合并 |
| 触发器 | 用户发起自然语言需求时自动激活 |

**职责清单：**
1. 接收用户需求，理解业务意图
2. 将需求拆解为可执行的任务单元
3. 分配任务给后端/测试 Agent 执行
4. 审核各 Agent 的输出，确保一致性
5. 合并各 Agent 的输出为完整交付
6. **不直接写业务代码**（只写配置、文档、规范文件）

**决策边界：**
- ✅ 可以决定技术方案、架构选型、库的选择
- ✅ 可以修改规范文件（soul.md / agents.md / project-context.md）
- ❌ 不直接编写业务逻辑 / API 接口 / 数据处理代码

### 2. Data Modeler（数据建模师）

| 属性 | 值 |
|------|-----|
| 角色名 | `data-modeler` |
| 技术栈 | .NET Core 8, FreeSql, ClosedXML |
| 输入 | Excel 数据字典 / 需求文档 |
| 输出 | 数据字典文档 / FreeSql 实体模型 / 数据库迁移脚本 / xlsx 导出模板 |

**职责清单：**
1. 定义 FreeSql 实体模型（Entity），配置 `[Table]` 和 `[Column]` 特性
2. 定义各数据库兼容的字段类型和命名策略
3. 设计数据库表关系和索引
4. 定义 xlsx 导出模板和对应实体映射

### 3. Backend Developer（后端开发者）

| 属性 | 值 |
|------|-----|
| 角色名 | `backend` |
| 技术栈 | .NET Core 8, Web API, FreeSql, JWT, Serilog, ClosedXML |
| 输入 | API 设计文档 / 数据字典 / 实体模型 |
| 输出 | API 代码 + Service 层代码 + Repository 层代码 + Middleware + 测试 |

**职责清单：**
1. 实现 JWT 认证中间件和 Token 签发接口
2. 实现项目/基桩/剖面/测点/报告的 CRUD 接口
3. 实现 FreeSql 多数据库切换配置
4. 实现 Serilog 双通道日志（文件+表）
5. 实现 xlsx 文件导出功能
6. 实现 RequestLoggingMiddleware（API 调用日志记录）
7. 编写单元测试和集成测试

**责任边界：**
- ✅ 可以根据数据字典调整实现细节
- ✅ 可以新增辅助方法和工具类
- ❌ 不修改已确定的数据字典字段定义
- ❌ 不修改 API 路径和响应格式规范

### 4. QA Engineer（测试工程师）

| 属性 | 值 |
|------|-----|
| 角色名 | `test` |
| 技术栈 | xUnit, Moq, Microsoft.AspNetCore.TestHost |
| 输入 | PR / 功能变更说明 |
| 输出 | 测试报告 + Bug 记录 |

**职责清单：**
1. 运行全套测试套件
2. 验证覆盖率阈值（Service ≥ 90%，整体 ≥ 80%）
3. 鉴权测试覆盖（无 token、过期、无效、有效）
4. 多数据库兼容性测试（SQLite / PostgreSQL）
5. 回归测试导出功能（xlsx 格式校验）
6. 边界测试（空项目、大量测点数据）
7. 将 Bug 记录到 `memory.md` 技术债务清单

## 二、协作协议

### 2.1 任务交接格式

```
---
from: <角色名>
to: <角色名>
task_id: <UUID>
context:
  - 数据字典引用: <模型/字段名>
  - 关联接口: <API 路径>
  - 技术约束: <约束列表>
deliverable: <期望产出>
deadline: <可选>
---
```

### 2.2 跨角色依赖规则

1. **接口优先** — 先定义 API 签名和数据模型，再实现业务逻辑
2. **数据模型固定** — Data Modeler 输出的实体定义和字段映射确定后，后端按此开发
3. **测试驱动** — 任何功能代码 merge 前必须有对应测试
4. **不跨域修改** — 各角色 Agent 不修改其他角色的文件
5. **冲突升级** — 发现规范冲突、架构分歧 → 升级给 Orchestrator 裁决

### 2.3 文件所有权映射

| 文件/目录 | 所有者 | 说明 |
|----------|--------|------|
| `doc/hermes/` | orchestrator | 项目规范文档 |
| `doc/需求/` | orchestrator | 需求文档 |
| `doc/data-dictionary/` | data-modeler | 数据字典 |
| `src/PileDetectionApi/Entities/` | data-modeler → backend | 实体模型（移交后 backend 可调整实现细节） |
| `src/PileDetectionApi/DTOs/` | backend | 数据传输对象 |
| `src/PileDetectionApi/Controllers/` | backend | API 控制器 |
| `src/PileDetectionApi/Services/` | backend | 业务逻辑 |
| `src/PileDetectionApi/Middleware/` | backend | JWT 认证 + 日志中间件 |
| `src/PileDetectionApi/Data/` | backend | FreeSql 配置 |
| `src/PileDetectionApi/Configs/` | backend | 强类型配置类 |
| `src/PileDetectionApi.Tests/` | backend / test | 测试代码 |
| `data/` | backend | SQLite 数据库运行时目录 |
| `logs/` | backend | 日志文件目录 |

## 三、会话启动协议

每次 Hermes 启动一个新会话，必须按以下顺序读取：

```
1. soul.md        → 理解设计哲学和编码风格
2. project-context.md → 了解技术栈和强制要求
3. agents.md      → 确定自己的角色和边界
4. memory.md      → 加载 ADR、技术债务、上下文锚点
5. hooks.md       → 了解自动执行的钩子
```

## 四、冲突处理

| 场景 | 处理方式 |
|------|----------|
| 规范冲突 | 以 `soul.md` 为准，更新其他文件 |
| 代码与规范冲突 | 以规范为准，修改代码 |
| Agent 间责任分歧 | 上报 Orchestrator 裁决 |
| 数据字典与实际不符 | 更新数据字典变更文件，原文件不变 |
| 数据库切换问题 | orchestrator 仲裁，确定测试覆盖的数据库类型 |
| JWT 鉴权方式变更 | orchestrator 审核，同步更新所有相关接口文档 |
