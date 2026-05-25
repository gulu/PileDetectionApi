namespace PileDetectionApi.DTOs.Response;

public class ApiKeyCreatedResponse
{
    public string ClientId { get; set; } = string.Empty;
    /// <summary>明文 API Key（仅创建时返回一次）</summary>
    public string ApiKey { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}

public class ApiKeyListResponse
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public int Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
