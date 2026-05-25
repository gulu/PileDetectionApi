using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>单桩报告表</summary>
[Table(Name = "pile_report")]
public class PileReportEntity
{
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }
    public Guid PileInfoId { get; set; }
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public int? IntegrityCategory { get; set; }
    public double? AvgVelocity { get; set; }
    public double? CriticalVelocity { get; set; }
    public double? AvgAmplitude { get; set; }
    public double? CriticalAmplitude { get; set; }
    public string? Conclusion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string ApiVersion { get; set; } = "v1";

    [Navigate(nameof(PileInfoId))]
    public PileInfoEntity? PileInfo { get; set; }
}
