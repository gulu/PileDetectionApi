# 项目记忆

## .NET SDK 环境
- .NET 8 SDK (8.0.421) 已安装到 `C:\dotnet8`
- `global.json` 锁定 SDK 版本为 8.0.421，`rollForward: latestPatch`
- **已知问题**：系统 `dotnet`（`C:\Program Files\dotnet`）为 SDK 10.0.300，NuGet 存在路径解析问题
- **解决方案**：所有 dotnet 命令需使用 `C:\dotnet8\dotnet.exe`，且加 `--no-restore` 参数。NuGet 包缓存在 `C:\Users\Administrator\.nuget\packages`
- **NuGet 还原**：需使用 nuget.exe（`C:\Users\Administrator\AppData\Local\Temp\nuget.exe`）单独执行 restore，或通过 `nuget restore PileDetectionApi.sln -PackagesDirectory "C:\Users\Administrator\.nuget\packages"` 还原

## 测试配置
- `[Trait("Category", "Integration")]` 标记集成测试
- 单元测试过滤器：`dotnet test --no-restore --filter "Category!=Integration"`
- 集成测试过滤器：`dotnet test --no-restore --filter "Category=Integration"`
- 测试规范：xUnit，使用 FreeSql 内存 SQLite（不使用 Moq），方法命名 `{方法}_{场景}_Should{预期}`

## 构建脚本
- `build.bat` - 正式构建（Release），使用 `C:\dotnet8\dotnet.exe`
- `test.bat` - 快速测试运行（Debug，支持自定义过滤器参数），使用 `C:\dotnet8\dotnet.exe`

## 权限控制
- 新增 `ProjectPermissionEntity` → `project_permission` 表，多对多关联 `api_key` ↔ `project_info`
- 新增 `POST /api/v1/projects/permitted-list` 端点，根据 JWT 中的 clientId 查询有权限的项目
- 服务层方法：`GetPermittedPagedAsync(string clientId, int page, int pageSize, string? keyword)`
