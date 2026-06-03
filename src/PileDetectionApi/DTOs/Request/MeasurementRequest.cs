namespace PileDetectionApi.DTOs.Request;

public class CreateMeasurementRequest
{
    public string Profile { get; set; } = string.Empty;
    public double Depth { get; set; }
    public double? SoundVelocity { get; set; }
    public double? Amplitude { get; set; }
    public double? SoundTime { get; set; }
    public double? Psd { get; set; }

    /// <summary>原始波形矩阵信息（可选）</summary>
    public MeasurementWaveformRequest? RawWaveform { get; set; }
}

public class UpdateMeasurementRequest
{
    public double? SoundVelocity { get; set; }
    public double? Amplitude { get; set; }
    public double? SoundTime { get; set; }
    public double? Psd { get; set; }

    /// <summary>原始波形矩阵信息（可选）</summary>
    public MeasurementWaveformRequest? RawWaveform { get; set; }
}

/// <summary>原始波形矩阵请求 DTO — 嵌套在测点请求中，可选</summary>
public class MeasurementWaveformRequest
{
    /// <summary>采样频率 (kHz)，默认 500.0</summary>
    public double? SamplingRate { get; set; }

    /// <summary>采样点数（如 512, 1024），默认 512</summary>
    public int? PointCount { get; set; }

    /// <summary>存储模式：1=数据库内联存储, 2=外部文件存储</summary>
    public int? StorageType { get; set; }

    /// <summary>模式1：直接存成数组JSON字符串 '[0.12, -0.45, 1.2, ...]'</summary>
    public string? RawPointsJson { get; set; }

    /// <summary>模式2：外部 .npy 或 .h5 文件路径</summary>
    public string? FilePath { get; set; }
}
