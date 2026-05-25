using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>API 密钥表</summary>
[Table(Name = "api_key")]
public class ApiKeyEntity
{
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string ApiKeyHash { get; set; } = string.Empty;
    public int Status { get; set; } = 1;
    public DateTime? ExpiresAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
