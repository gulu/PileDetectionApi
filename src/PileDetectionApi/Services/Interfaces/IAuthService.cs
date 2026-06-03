using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthTokenResponse> GenerateTokenAsync(string apiKey, string clientId);

    Task<AuthTokenResponse> GenerateTokenByUserIdNameAsync(string clientId, string clientName);
}
