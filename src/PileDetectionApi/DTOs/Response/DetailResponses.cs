namespace PileDetectionApi.DTOs.Response;

public class ProfileStatResponse
{
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
}

public class MeasurementResponse
{
    public Guid Id { get; set; }
    public Guid PileInfoId { get; set; }
    public string Profile { get; set; } = string.Empty;
    public double Depth { get; set; }
    public double? SoundVelocity { get; set; }
    public double? Amplitude { get; set; }
    public double? SoundTime { get; set; }
    public double? Psd { get; set; }

    /// <summary>是否包含原始波形矩阵数据</summary>
    public bool HasWaveform { get; set; }
}

/// <summary>原始波形矩阵响应 DTO</summary>
public class MeasurementWaveformResponse
{
    public Guid MeasurementDataId { get; set; }
    public Guid PileInfoId { get; set; }
    public double SamplingRate { get; set; }
    public int PointCount { get; set; }
    public int StorageType { get; set; }
    public string? RawPointsJson { get; set; }
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PileReportResponse
{
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
}

public class ProjectReportResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? Conclusion { get; set; }
    public string? IntegritySummary { get; set; }
}
