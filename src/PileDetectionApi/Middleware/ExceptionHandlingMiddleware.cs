using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "资源未找到");
            await WriteErrorResponse(context, 404, "资源不存在", ex.Message);
        }
        catch (DuplicateWaitObjectException ex)
        {
            _logger.LogWarning(ex, "数据重复");
            await WriteErrorResponse(context, 409, "数据重复", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "未授权访问");
            await WriteErrorResponse(context, 401, "未授权", ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "参数错误");
            await WriteErrorResponse(context, 400, "参数错误", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "服务器内部错误");
            // 开发环境返回详细错误信息便于调试
            var detail = context.RequestServices.GetService<IWebHostEnvironment>()?.IsDevelopment() == true
                ? $"{ex.GetType().Name}: {ex.Message}"
                : "请联系管理员";
            await WriteErrorResponse(context, 500, "服务器内部错误", detail);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message, string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = ApiResponse<object>.Fail(statusCode, message, new() { { "detail", detail } });
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
