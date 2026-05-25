namespace PileDetectionApi.DTOs.Request;

public class AuthTokenRequest
{
    public string ApiKey { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}
