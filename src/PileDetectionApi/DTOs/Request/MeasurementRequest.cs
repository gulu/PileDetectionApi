namespace PileDetectionApi.DTOs.Request;

public class CreateMeasurementRequest
{
    public string Profile { get; set; } = string.Empty;
    public double Depth { get; set; }
    public double? SoundVelocity { get; set; }
    public double? Amplitude { get; set; }
    public double? SoundTime { get; set; }
    public double? Psd { get; set; }
}

public class UpdateMeasurementRequest
{
    public double? SoundVelocity { get; set; }
    public double? Amplitude { get; set; }
    public double? SoundTime { get; set; }
    public double? Psd { get; set; }
}
