# 项目记忆

## .NET SDK 环境
- .NET 8 SDK (8.0.420) 已安装到 `C:\dotnet8`
- `global.json` 锁定 SDK 版本为 8.0.420
- **已知问题**：在 Git Bash 中运行 `dotnet restore` 失败（`Value cannot be null. Parameter 'path1'`），原因是 NuGet.Common 无法解析 `CommonApplicationData` 路径。
- **解决方案**：所有 dotnet 命令需加 `--no-restore` 参数。NuGet 包已缓存在 `D:\.nuget\packages`

## 测试配置
- `[Trait("Category", "Integration")]` 标记集成测试
- 单元测试过滤器：`dotnet test --no-restore --filter "Category!=Integration"`
- 集成测试过滤器：`dotnet test --no-restore --filter "Category=Integration"`
- 测试规范：xUnit + Moq，方法命名 `{方法}_{场景}_Should{预期}`

## 构建脚本
- `build.bat` - 正式构建（Release）
- `test.bat` - 快速测试运行（Debug，支持自定义过滤器参数）
