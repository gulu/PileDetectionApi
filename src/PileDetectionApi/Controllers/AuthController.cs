using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>获取 JWT Token（无需鉴权）</summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), 200)]
    public async Task<IActionResult> GetToken([FromBody] AuthTokenRequest request)
    {
        var result = await _authService.GenerateTokenAsync(request.ApiKey, request.ClientId);
        return Ok(ApiResponse<AuthTokenResponse>.Ok(result));
    }
}
