namespace PileDetectionApi.DTOs.Request;

public class CreateProjectReportRequest
{
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? Conclusion { get; set; }
    public string? IntegritySummary { get; set; }
}

public class UpdateProjectReportRequest
{
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? Conclusion { get; set; }
    public string? IntegritySummary { get; set; }
}
