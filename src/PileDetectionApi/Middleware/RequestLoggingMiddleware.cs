using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FreeSql;
using PileDetectionApi.Entities;

namespace PileDetectionApi.Middleware;

/// <summary>
/// 记录每次 API 调用到 api_log 表
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private static readonly string[] IgnorePaths = { "/swagger", "/favicon" };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        if (IgnorePaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();

        // 读取请求体
        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
        context.Request.Body.Position = 0;

        // 捕获响应体
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            responseBodyStream.Position = 0;
            var responseBody = await new StreamReader(responseBodyStream, Encoding.UTF8).ReadToEndAsync();
            responseBodyStream.Position = 0;
            await responseBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            // 异步写入日志（不阻塞响应）
            _ = WriteLogAsync(context, requestBody, responseBody, sw.ElapsedMilliseconds);
        }
    }

    private async Task WriteLogAsync(HttpContext context, string requestBody, string responseBody, long durationMs)
    {
        try
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "";
            var log = new ApiLogEntity
            {
                Endpoint = context.Request.Path,
                HttpMethod = context.Request.Method,
                RequestBody = Truncate(requestBody, 8000),
                ResponseCode = context.Response.StatusCode,
                ResponseBody = Truncate(responseBody, 8000),
                ClientIp = ip,
                DurationMs = durationMs,
                CreatedAt = DateTime.UtcNow
            };

            // 通过 DI 获取 FreeSql 实例
            var fsql = context.RequestServices.GetRequiredService<IFreeSql>();
            await fsql.Insert(log).ExecuteAffrowsAsync();

            // 同时写入 Serilog 文件日志（含请求/响应详情）
            _logger.LogInformation(
                "[API] {Method} {Path} → {StatusCode} ({DurationMs}ms)\n  ClientIp: {Ip}\n  Request: {Request}\n  Response: {Response}",
                context.Request.Method, context.Request.Path,
                context.Response.StatusCode, durationMs,
                ip,
                Truncate(requestBody, 2000),
                Truncate(responseBody, 2000));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "写入 API 日志失败");
        }
    }

    private static string Truncate(string? value, int maxLength)
        => value?.Length > maxLength ? value[..maxLength] + "..." : value ?? "";
}
