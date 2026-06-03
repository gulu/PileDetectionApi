using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IAdminAuthService
{
    /// <summary>验证管理员主密钥</summary>
    bool ValidateMasterKey(string masterKey);

    Task<ApiKeyCreatedResponse> CreateApiInfoAsync(string clientId,string clientName, int expireDays);
    /// <summary>生成新的 API Key</summary>
    Task<ApiKeyCreatedResponse> CreateApiKeyAsync(string clientName, int expireDays);
    /// <summary>列出所有 API Key</summary>
    Task<List<ApiKeyListResponse>> ListApiKeysAsync();
    /// <summary>切换 Key 状态（启用/禁用）</summary>
    Task<bool> ToggleKeyStatusAsync(Guid id);
}
