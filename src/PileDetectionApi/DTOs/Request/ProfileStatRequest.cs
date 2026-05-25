namespace PileDetectionApi.DTOs.Request;

public class CreateProfileStatRequest
{
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

public class UpdateProfileStatRequest
{
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
