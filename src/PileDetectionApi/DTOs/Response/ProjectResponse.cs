namespace PileDetectionApi.DTOs.Response;

public class ProjectResponse
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectNo { get; set; }
    public string? ProjectLocation { get; set; }
    public string? ProjectManager { get; set; }
    public string? ProjectDesc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProjectDetailResponse
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectNo { get; set; }
    public string? ProjectLocation { get; set; }
    public string? ProjectManager { get; set; }
    public string? ProjectDesc { get; set; }
    public int PileCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
