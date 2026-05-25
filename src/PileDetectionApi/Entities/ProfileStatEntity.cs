using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>剖面统计表</summary>
[Table(Name = "profile_stat")]
public class ProfileStatEntity
{
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }
    public Guid PileInfoId { get; set; }
    public string Profile { get; set; } = string.Empty;
    public double? Distance { get; set; }
    public double? MaxVelocity { get; set; }
    public double? MinVelocity { get; set; }
    public double? AvgVelocity { get; set; }
    public double? StdVelocity { get; set; }
    public double? CvVelocity { get; set; }
    public double? CriticalVelocity { get; set; }
    public double? MaxAmplitude { get; set; }
    public double? MinAmplitude { get; set; }
    public double? AvgAmplitude { get; set; }
    public double? StdAmplitude { get; set; }
    public double? CvAmplitude { get; set; }
    public double? CriticalAmplitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string ApiVersion { get; set; } = "v1";

    [Navigate(nameof(PileInfoId))]
    public PileInfoEntity? PileInfo { get; set; }
}
