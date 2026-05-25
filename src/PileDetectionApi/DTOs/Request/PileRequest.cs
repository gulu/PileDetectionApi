namespace PileDetectionApi.DTOs.Request;

public class CreatePileRequest
{
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
}

public class UpdatePileRequest
{
    public string? PileName { get; set; }
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
}
