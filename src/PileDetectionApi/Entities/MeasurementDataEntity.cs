using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>测点数据表</summary>
[Table(Name = "measurement_data")]
public class MeasurementDataEntity
{
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }
    public Guid PileInfoId { get; set; }
    public string Profile { get; set; } = string.Empty;
    public double Depth { get; set; }
    public double? SoundVelocity { get; set; }
    public double? Amplitude { get; set; }
    public double? SoundTime { get; set; }
    public double? Psd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string ApiVersion { get; set; } = "v1";

    [Navigate(nameof(PileInfoId))]
    public PileInfoEntity? PileInfo { get; set; }
}
