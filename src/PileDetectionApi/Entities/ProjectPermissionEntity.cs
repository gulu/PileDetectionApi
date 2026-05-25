using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>项目处理权限表（多对多：api_key ↔ project_info）</summary>
[Table(Name = "project_permission")]
public class ProjectPermissionEntity
{
    /// <summary>主键 ID</summary>
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }

    /// <summary>客户端标识（关联 api_key.client_id）</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>项目 ID（关联 project_info.Id）</summary>
    public Guid ProjectId { get; set; }

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
