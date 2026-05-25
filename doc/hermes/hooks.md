# Hermes 钩子规范

> 钩子是 Hermes Agent 在特定事件触发时自动执行的操作。
> 用于保证代码质量和流程合规。

## 运行前钩子

| 操作 | 说明 |
|------|------|
| 需求文件检查 | 检查 `doc/需求/` 下是否有最新的需求文档和数据字典 |
| 规范加载 | 按序加载 soul.md → project-context.md → agents.md → memory.md |
| 环境检查 | 确认 .NET SDK 版本 ≥ 8.0，已安装必要 NuGet 包 |

## 运行后钩子

| 操作 | 说明 |
|------|------|
| 测试覆盖率报告 | 检查测试覆盖率，确认 Service 层 ≥ 90%，整体 ≥ 80% |
| 数据字典同步检查 | 检查新增字段是否已在数据字典中记录 |
| DTO 与 Entity 一致性 | 检查新增 DTO 字段是否同步更新了 AutoMapper MappingProfile |
| FreeSql 实体同步检查 | 检查新增 Entity 是否注册到 FreeSql CodeFirst 配置 |
| 构建验证 | 运行 `dotnet build` 确保编译通过 |
| 规范合规检查 | 检查代码命名是否遵循 soul.md 规范 |
| 多数据库兼容性检查 | 确认 SQLite 下可用，逻辑不包含数据库特定语法 |

## Agent 交互钩子

| 触发时机 | 操作 |
|----------|------|
| 会话启动 | 按顺序读取 `soul.md` → `project-context.md` → `agents.md` → `memory.md` |
| 新建 C# 文件 | 检查命名是否符合 PascalCase 规范 |
| 新建 Entity 文件 | 检查是否配置了 `[Table]`、`[Column]` 特性 |
| 新建 Controller | 检查是否使用了 `[Authorize]` 特性（AuthController 除外） |
| 新增 API 端点 | 检查是否有对应的 DTO 和 Validator |
| 新增 Service 方法 | 检查是否编写了对应的单元测试用例 |
| 修改数据字典 | 检查是否同步更新了对应的 DTO 和 Mapping 配置 |
| 提交代码前 | 运行 `dotnet build` + `dotnet test` 确保通过 |

## 环境检查钩子

| 检查项 | 说明 |
|--------|------|
| .NET SDK 版本 | 确认 ≥ 8.0（运行 `dotnet --list-sdks`） |
| NuGet 包完整性 | 确认项目所需包已还原（运行 `dotnet restore`） |
| SQLite 可写性 | 确认数据库文件目录存在且有写入权限 |
| JWT Secret 配置 | 确认 `JWT_SECRET` 环境变量或 appsettings 中已配置 |
| 测试项目可构建 | 确认测试项目能正常编译 |
| Git 仓库状态 | 确认不在冲突状态，工作区干净 |
| xlsx 模板可读 | 确认 `doc/需求/pile1.xlsx` 存在可作为导出格式参考 |
