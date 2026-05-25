namespace PileDetectionApi.Configs;

public class JwtConfig
{
    public string Secret { get; set; } = "DefaultSecretKey_ChangeMe_InProduction_32Chars!";
    public string Issuer { get; set; } = "PileDetectionApi";
    public string Audience { get; set; } = "PileDetectionClient";
    public int ExpireMinutes { get; set; } = 1440; // 24 小时
}
