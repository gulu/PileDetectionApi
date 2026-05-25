using System.Security.Cryptography;
using System.Text;
using FreeSql;
using Microsoft.Extensions.Options;
using PileDetectionApi.Configs;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly IFreeSql _fsql;
    private readonly AdminConfig _adminConfig;
    private readonly ILogger<AuthService> _logger;

    public AdminAuthService(IFreeSql fsql, IOptions<AdminConfig> adminConfig, ILogger<AuthService> logger)
    {
        _fsql = fsql;
        _adminConfig = adminConfig.Value;
        _logger = logger;
    }

    public bool ValidateMasterKey(string masterKey)
    {
        return !string.IsNullOrWhiteSpace(masterKey)
            && masterKey == _adminConfig.MasterKey;
    }


    /// <summary>
    /// 手动创建登录验证的账号 和 姓名和 过期日期 
    /// </summary>
    /// <param name="clientName">用户名</param>
    /// <param name="clientId">账号</param>
    /// <param name="expireDays">过期日期</param>
    /// <returns></returns>
    public async Task<ApiKeyCreatedResponse> CreateApiInfoAsync(string clientId, string clientName, int expireDays)
    {
        var loginfo = $"CreateApiInfoAsync  已签发: clientId={clientId},clientName={clientName} ,expireDays={expireDays}";
        
        try
        {
            _logger.LogInformation(loginfo);

            // 生成 48 字节随机密钥，转为 Base64（URL safe）
            var rawKeyBytes = RandomNumberGenerator.GetBytes(48);
            var rawKey = Convert.ToBase64String(rawKeyBytes)
                .Replace("/", "_").Replace("+", "-").TrimEnd('=');
            var apiKeyPlain = $"pile_sk_{rawKey}";

            // 计算 SHA256 哈希
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKeyPlain));
            var apiKeyHash = Convert.ToHexString(hash).ToLowerInvariant();

            var expiresAt = expireDays > 0
                ? (DateTime?)DateTime.UtcNow.AddDays(expireDays)
                : null;

            var entity = new ApiKeyEntity
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                ClientName = clientName,
                ApiKeyHash = apiKeyHash,
                Status = 1,
                ExpiresAt = expiresAt,
                CreatedBy = "admin"
            };

            await _fsql.Insert(entity).ExecuteAffrowsAsync();

            return new ApiKeyCreatedResponse
            {
                ClientId = clientId,
                ApiKey = apiKeyPlain,
                ClientName = clientName,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex) {
            _logger.LogInformation("Token 已签发: clientId={ClientId}, 有效期至 {ExpiresAt}", clientId, expiresAt);
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="expireDays"></param>
    /// <returns></returns>
    public async Task<ApiKeyCreatedResponse> CreateApiKeyAsync(string clientName, int expireDays)
    {
        
        var clientId = $"client_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..8]}";

        // 生成 48 字节随机密钥，转为 Base64（URL safe）
        var rawKeyBytes = RandomNumberGenerator.GetBytes(48);
        var rawKey = Convert.ToBase64String(rawKeyBytes)
            .Replace("/", "_").Replace("+", "-").TrimEnd('=');
        var apiKeyPlain = $"pile_sk_{rawKey}";

        // 计算 SHA256 哈希
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKeyPlain));
        var apiKeyHash = Convert.ToHexString(hash).ToLowerInvariant();

        var expiresAt = expireDays > 0
            ? (DateTime?)DateTime.UtcNow.AddDays(expireDays)
            : null;

        var entity = new ApiKeyEntity
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ClientName = clientName,
            ApiKeyHash = apiKeyHash,
            Status = 1,
            ExpiresAt = expiresAt,
            CreatedBy = "admin"
        };

        await _fsql.Insert(entity).ExecuteAffrowsAsync();

        return new ApiKeyCreatedResponse
        {
            ClientId = clientId,
            ApiKey = apiKeyPlain,
            ClientName = clientName,
            ExpiresAt = expiresAt
        };
    }

    public async Task<List<ApiKeyListResponse>> ListApiKeysAsync()
    {
        return await _fsql.Select<ApiKeyEntity>()
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(k => new ApiKeyListResponse
            {
                Id = k.Id,
                ClientId = k.ClientId,
                ClientName = k.ClientName,
                Status = k.Status,
                ExpiresAt = k.ExpiresAt,
                CreatedAt = k.CreatedAt
            });
    }

    public async Task<bool> ToggleKeyStatusAsync(Guid id)
    {
        var key = await _fsql.Select<ApiKeyEntity>().Where(k => k.Id == id).FirstAsync();
        if (key == null) return false;

        key.Status = key.Status == 1 ? 0 : 1;
        key.UpdatedAt = DateTime.UtcNow;
        await _fsql.Update<ApiKeyEntity>().SetSource(key).ExecuteAffrowsAsync();
        return true;
    }
}
