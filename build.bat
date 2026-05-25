@echo off
chcp 65001 >nul
set DOTNET_ROOT=C:\dotnet8
set PATH=%DOTNET_ROOT%;%PATH%
echo ========================================
echo   PileDetectionApi - 构建脚本
echo ========================================
echo.

echo [1/3] 编译项目...
cd /d "%~dp0"
"%DOTNET_ROOT%\dotnet" build src/PileDetectionApi/PileDetectionApi.csproj -c Release --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo 编译失败
    pause
    exit /b 1
)

echo [2/3] 运行单元测试...
"%DOTNET_ROOT%\dotnet" test src/PileDetectionApi.Tests/PileDetectionApi.Tests.csproj -c Release --no-restore --no-build --filter "Category!=Integration"
if %ERRORLEVEL% NEQ 0 (
    echo 单元测试失败
    pause
    exit /b 1
)

echo.
echo ========================================
echo   单元测试通过!
echo.
echo   如需运行整体功能测试（灌入正式数据）:
echo   "%DOTNET_ROOT%\dotnet" test src/PileDetectionApi.Tests\PileDetectionApi.Tests.csproj --no-restore --filter "SeedDataTests"
echo.
echo   正式数据：从 doc/需求/pile1.xlsx 读取
echo   通过 API 写入 dbdata/pile.db
echo   用于生成 API 集成文档
echo ========================================
echo.
echo ========================================
echo   构建成功!
echo   数据库文件: dbdata/pile.db
echo   日志目录: Logs/
echo ========================================
pause
