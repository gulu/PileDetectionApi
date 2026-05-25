namespace PileDetectionApi.DTOs.Request;

public class CreateApiKeyRequest
{
    public string ClientName { get; set; } = string.Empty;
    public int ExpireDays { get; set; } = 365;
}

public class AdminAuthRequest
{
    public string MasterKey { get; set; } = string.Empty;
}
