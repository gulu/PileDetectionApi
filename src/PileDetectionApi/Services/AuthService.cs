using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FreeSql;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PileDetectionApi.Configs;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class AuthService : IAuthService
{
    private readonly IFreeSql _fsql;
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IFreeSql fsql, IOptions<JwtConfig> jwtConfig, ILogger<AuthService> logger)
    {
        _fsql = fsql;
        _jwtConfig = jwtConfig.Value;
        _logger = logger;
    }


    public async Task<AuthTokenResponse> GenerateTokenByUserIdNameAsync(string clientId, string clientName)
    {
        var loginfo = @"";
        try
        {

            if (string.IsNullOrWhiteSpace(clientId))
            {
                loginfo = "clientId 不能为空";
                _logger.LogInformation(loginfo);
                throw new UnauthorizedAccessException(loginfo);
            }
               
            if (string.IsNullOrWhiteSpace(clientName))
            {
                loginfo = "clientName 不能为空";
                _logger.LogInformation(loginfo);
                throw new ArgumentException(loginfo);
            }


            // 查询数据库验证
            var validKey = await _fsql.Select<ApiKeyEntity>()
                .Where(k => k.ClientId == clientId
                         && k.ClientName == clientName
                         && k.Status == 1
                         && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
                .AnyAsync();

            if (!validKey)
            {
                loginfo = "clientName 或 clientId 无效或已过期";
                _logger.LogInformation(loginfo);
                throw new UnauthorizedAccessException(loginfo);
            }

            var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? _jwtConfig.Secret;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpireMinutes);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, clientId),
            new Claim(ClaimTypes.Role, "client"),
            new Claim("clientId", clientId)
        };

            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Token 已签发: clientId={ClientId}, 有效期至 {ExpiresAt}", clientId, expiresAt);

            return new AuthTokenResponse
            {
                Token = tokenStr,
                ExpiresAt = expiresAt,
                TokenType = "Bearer"
            };
        }
        catch (Exception ex)
        {
            loginfo = "GenerateTokenByUserIdNameAsync 错误";
            
            _logger.LogError(ex, loginfo);

            return null;
        }
    }

    public async Task<AuthTokenResponse> GenerateTokenAsync(string apiKey, string clientId)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new UnauthorizedAccessException("API Key 不能为空");
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("客户端 ID 不能为空");

        // 计算传入 API Key 的 SHA256 哈希
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        var apiKeyHash = Convert.ToHexString(hash).ToLowerInvariant();

        // 查询数据库验证
        var validKey = await _fsql.Select<ApiKeyEntity>()
            .Where(k => k.ClientId == clientId
                     && k.ApiKeyHash == apiKeyHash
                     && k.Status == 1
                     && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            .AnyAsync();

        if (!validKey)
            throw new UnauthorizedAccessException("API Key 无效或已过期");

        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? _jwtConfig.Secret;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpireMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, clientId),
            new Claim(ClaimTypes.Role, "client"),
            new Claim("clientId", clientId)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("Token 已签发: clientId={ClientId}, 有效期至 {ExpiresAt}", clientId, expiresAt);

        return new AuthTokenResponse
        {
            Token = tokenStr,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }
}
