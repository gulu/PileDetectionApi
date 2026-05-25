namespace PileDetectionApi.DTOs.Response;

public class PileResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string PileName { get; set; } = string.Empty;
    public double? DesignLength { get; set; }
    public int? DesignDiameter { get; set; }
    public string? DesignStrength { get; set; }
    public DateTime? PourDate { get; set; }
    public DateTime? TestDate { get; set; }
    public string? TestStandard { get; set; }
    public string? InstrumentModel { get; set; }
    public string? InstrumentSn { get; set; }
    public string? CertificationNo { get; set; }
    public string? Tester { get; set; }
    public string? TesterCertNo { get; set; }
    public int? IntegrityCategory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PileDetailResponse : PileResponse
{
    public List<ProfileStatResponse> ProfileStats { get; set; } = new();
    public List<MeasurementResponse> Measurements { get; set; } = new();
    public PileReportResponse? Report { get; set; }
}

public class PileSummaryResponse
{
    public Guid Id { get; set; }
    public string PileName { get; set; } = string.Empty;
    public double? DesignLength { get; set; }
    public int? DesignDiameter { get; set; }
    public int? IntegrityCategory { get; set; }
    public DateTime CreatedAt { get; set; }
}
