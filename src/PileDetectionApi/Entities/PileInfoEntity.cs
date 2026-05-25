using FreeSql.DataAnnotations;

namespace PileDetectionApi.Entities;

/// <summary>基桩信息表</summary>
[Table(Name = "pile_info")]
public class PileInfoEntity
{
    /// <summary>主键 ID</summary>
    [Column(IsPrimary = true)]
    public Guid Id { get; set; }

    /// <summary>所属项目 ID</summary>
    public Guid ProjectId { get; set; }

    /// <summary>桩号</summary>
    public string PileName { get; set; } = string.Empty;

    /// <summary>设计桩长（m）</summary>
    public double? DesignLength { get; set; }

    /// <summary>设计桩径（mm）</summary>
    public int? DesignDiameter { get; set; }

    /// <summary>设计强度等级</summary>
    public string? DesignStrength { get; set; }

    /// <summary>浇筑日期</summary>
    public DateTime? PourDate { get; set; }

    /// <summary>检测日期</summary>
    public DateTime? TestDate { get; set; }

    /// <summary>检测标准</summary>
    public string? TestStandard { get; set; }

    /// <summary>仪器型号</summary>
    public string? InstrumentModel { get; set; }

    /// <summary>仪器编号</summary>
    public string? InstrumentSn { get; set; }

    /// <summary>设备检定证书号</summary>
    public string? CertificationNo { get; set; }

    /// <summary>检测人</summary>
    public string? Tester { get; set; }

    /// <summary>检测人证书号</summary>
    public string? TesterCertNo { get; set; }

    /// <summary>完整性类别</summary>
    public int? IntegrityCategory { get; set; }

    /// <summary>软删除标记</summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>API 版本</summary>
    public string ApiVersion { get; set; } = "v1";

    [Navigate(nameof(ProjectId))]
    public ProjectInfoEntity? Project { get; set; }

    [Navigate(nameof(ProfileStatEntity.PileInfoId))]
    public List<ProfileStatEntity> ProfileStats { get; set; } = new();

    [Navigate(nameof(MeasurementDataEntity.PileInfoId))]
    public List<MeasurementDataEntity> Measurements { get; set; } = new();

    [Navigate(nameof(PileReportEntity.PileInfoId))]
    public PileReportEntity? Report { get; set; }
}
