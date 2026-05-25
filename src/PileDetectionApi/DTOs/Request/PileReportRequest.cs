namespace PileDetectionApi.DTOs.Request;

public class CreatePileReportRequest
{
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public int? IntegrityCategory { get; set; }
    public double? AvgVelocity { get; set; }
    public double? CriticalVelocity { get; set; }
    public double? AvgAmplitude { get; set; }
    public double? CriticalAmplitude { get; set; }
    public string? Conclusion { get; set; }
}

public class UpdatePileReportRequest
{
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public int? IntegrityCategory { get; set; }
    public double? AvgVelocity { get; set; }
    public double? CriticalVelocity { get; set; }
    public double? AvgAmplitude { get; set; }
    public double? CriticalAmplitude { get; set; }
    public string? Conclusion { get; set; }
}
