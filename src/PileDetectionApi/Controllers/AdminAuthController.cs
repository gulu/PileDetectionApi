using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _adminAuth;

    public AdminAuthController(IAdminAuthService adminAuth)
    {
        _adminAuth = adminAuth;
    }

    /// <summary>生成新的 API Key（需要 MasterKey 验证）</summary>
    [HttpPost("api-keys")]
    [ProducesResponseType(typeof(ApiResponse<ApiKeyCreatedResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> CreateApiKey(
        [FromHeader(Name = "X-Master-Key")] string masterKey,
        [FromBody] CreateApiKeyRequest request)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        var result = await _adminAuth.CreateApiKeyAsync(request.ClientName, request.ExpireDays);
        return Ok(ApiResponse<ApiKeyCreatedResponse>.Ok(result));
    }

    /// <summary>列出所有 API Key</summary>
    [HttpPost("api-keys/list")]
    [ProducesResponseType(typeof(ApiResponse<List<ApiKeyListResponse>>), 200)]
    public async Task<IActionResult> ListApiKeys(
        [FromHeader(Name = "X-Master-Key")] string masterKey)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        var keys = await _adminAuth.ListApiKeysAsync();
        return Ok(ApiResponse<List<ApiKeyListResponse>>.Ok(keys));
    }

    /// <summary>启用/禁用 API Key</summary>
    [HttpPost("api-keys/{id}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ToggleKeyStatus(
        Guid id,
        [FromHeader(Name = "X-Master-Key")] string masterKey)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        var ok = await _adminAuth.ToggleKeyStatusAsync(id);
        if (!ok)
            return NotFound(ApiResponse<object>.Fail(404, "API Key 不存在"));

        return Ok(ApiResponse<object>.Ok(new { }, "状态已切换"));
    }
}
