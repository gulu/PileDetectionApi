using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>项目报告表</summary>
[Table(Name = "project_report")]
public class ProjectReportEntity
{
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? Conclusion { get; set; }
    public string? IntegritySummary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string ApiVersion { get; set; } = "v1";

    [Navigate(nameof(ProjectId))]
    public ProjectInfoEntity? Project { get; set; }
}
