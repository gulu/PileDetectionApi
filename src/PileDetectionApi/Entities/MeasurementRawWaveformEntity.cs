using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>原始波形矩阵表 — 与 measurement_data 1:1 可选关联</summary>
[Table(Name = "measurement_raw_waveform")]
public class MeasurementRawWaveformEntity
{
    /// <summary>主键，同时关联原测点表主键</summary>
    [Column(IsPrimary = true)]
    public Guid MeasurementDataId { get; set; }

    /// <summary>冗余基桩ID</summary>
    public Guid PileInfoId { get; set; }

    /// <summary>采样频率 (kHz)，默认 500.0</summary>
    public double SamplingRate { get; set; } = 500.0;

    /// <summary>采样点数（如 512, 1024），默认 512</summary>
    public int PointCount { get; set; } = 512;

    /// <summary>存储模式：1=数据库内联存储, 2=外部文件存储</summary>
    public int StorageType { get; set; } = 1;

    /// <summary>模式1：直接存成数组JSON字符串 '[0.12, -0.45, 1.2, ...]'</summary>
    public string? RawPointsJson { get; set; }

    /// <summary>模式2：外部 .npy 或 .h5 文件路径</summary>
    public string? FilePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Navigate(nameof(MeasurementDataId))]
    public MeasurementDataEntity? MeasurementData { get; set; }
}
