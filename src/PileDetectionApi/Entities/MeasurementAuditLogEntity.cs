using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>测点数据变更审计日志表</summary>
[Table(Name = "measurement_audit_log")]
public class MeasurementAuditLogEntity
{
    /// <summary>主键 ID</summary>
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }

    /// <summary>被操作的测点数据 ID</summary>
    public Guid MeasurementId { get; set; }

    /// <summary>操作类型：INSERT / UPDATE / DELETE</summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>操作人员 clientId</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>操作前数据（JSON，UPDATE/DELETE 时记录）</summary>
    public string? PreviousData { get; set; }

    /// <summary>操作后数据（JSON，INSERT/UPDATE 时记录）</summary>
    public string? NewData { get; set; }

    /// <summary>操作时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
