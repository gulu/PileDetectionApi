namespace PileDetectionApi.DTOs.Request;

public class CreateApiKeyRequest
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public int ExpireDays { get; set; } = 365;
}

public class AdminAuthRequest
{
    public string MasterKey { get; set; } = string.Empty;
}
