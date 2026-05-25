namespace PileDetectionApi.DTOs.Request;

/// <summary>授予单个项目权限请求</summary>
public class GrantPermissionRequest
{
    public string ClientId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
}

/// <summary>批量授予项目权限请求</summary>
public class BatchGrantPermissionRequest
{
    public string ClientId { get; set; } = string.Empty;
    public List<Guid> ProjectIds { get; set; } = new();
}

/// <summary>查询权限列表请求</summary>
public class ListPermissionsRequest
{
    public string? ClientId { get; set; }
}
