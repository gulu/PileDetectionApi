namespace PileDetectionApi.DTOs.Response;

public class ProjectPermissionResponse
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
