using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>项目信息表</summary>
[Table(Name = "project_info")]
public class ProjectInfoEntity
{
    /// <summary>主键 ID</summary>
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }

    /// <summary>项目名称</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>项目编号</summary>
    public string? ProjectNo { get; set; }

    /// <summary>项目地点</summary>
    public string? ProjectLocation { get; set; }

    /// <summary>项目负责人</summary>
    public string? ProjectManager { get; set; }

    /// <summary>项目描述</summary>
    public string? ProjectDesc { get; set; }

    /// <summary>软删除标记</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>API 版本</summary>
    public string ApiVersion { get; set; } = "v1";

    [Navigate(nameof(PileInfoEntity.ProjectId))]
    public List<PileInfoEntity> Piles { get; set; } = new();
}
