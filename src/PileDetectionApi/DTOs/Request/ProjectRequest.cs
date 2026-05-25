namespace PileDetectionApi.DTOs.Request;

public class CreateProjectRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectNo { get; set; }
    public string? ProjectLocation { get; set; }
    public string? ProjectManager { get; set; }
    public string? ProjectDesc { get; set; }
}

public class UpdateProjectRequest
{
    public string? ProjectName { get; set; }
    public string? ProjectNo { get; set; }
    public string? ProjectLocation { get; set; }
    public string? ProjectManager { get; set; }
    public string? ProjectDesc { get; set; }
}
