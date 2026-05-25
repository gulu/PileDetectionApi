using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>API 日志表</summary>
[Table(Name = "api_log")]
public class ApiLogEntity
{
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }
    public string? Endpoint { get; set; }
    public string? HttpMethod { get; set; }
    public string? RequestBody { get; set; }
    public int? ResponseCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ClientIp { get; set; }
    public long? DurationMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
