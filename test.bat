@echo off
chcp 65001 >nul
set DOTNET_ROOT=C:\dotnet8
set PATH=%DOTNET_ROOT%;%PATH%
echo ========================================
echo   PileDetectionApi - 测试运行脚本
echo   (.NET 8.0 SDK)
echo ========================================
echo.

cd /d "%~dp0"

echo [1/2] 编译项目（跳过恢复，使用缓存包）...
"%DOTNET_ROOT%\dotnet" build src/PileDetectionApi/PileDetectionApi.csproj -c Debug --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo 编译失败
    pause
    exit /b 1
)

"%DOTNET_ROOT%\dotnet" build src/PileDetectionApi.Tests/PileDetectionApi.Tests.csproj -c Debug --no-restore
if %ERRORLEVEL% NEQ 0 (
    echo 测试项目编译失败
    pause
    exit /b 1
)

echo.
echo [2/2] 运行单元测试...
if "%1"=="" (
    "%DOTNET_ROOT%\dotnet" test src/PileDetectionApi.Tests/PileDetectionApi.Tests.csproj -c Debug --no-restore --no-build --filter "Category!=Integration"
) else (
    "%DOTNET_ROOT%\dotnet" test src/PileDetectionApi.Tests/PileDetectionApi.Tests.csproj -c Debug --no-restore --no-build --filter "%*"
)

if %ERRORLEVEL% NEQ 0 (
    echo 测试失败
    pause
    exit /b 1
)

echo.
echo ========================================
echo   测试通过!
echo ========================================
pause
