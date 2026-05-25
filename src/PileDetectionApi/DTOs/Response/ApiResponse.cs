namespace PileDetectionApi.DTOs.Response;

/// <summary>
/// 统一 API 响应格式
/// </summary>
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = "success";
    public T? Data { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");

    public static ApiResponse<T> Ok(T data, string message = "success")
        => new() { Code = 200, Message = message, Data = data };

    public static ApiResponse<T> Created(T data, string message = "创建成功")
        => new() { Code = 201, Message = message, Data = data };

    public static ApiResponse<T> Fail(int code, string message, Dictionary<string, string>? errors = null)
        => new() { Code = code, Message = message, Errors = errors };
}

/// <summary>
/// 分页响应
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
