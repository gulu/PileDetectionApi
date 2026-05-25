using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Route("api/v1/admin/project-permissions")]
public class AdminProjectPermissionController : ControllerBase
{
    private readonly IAdminAuthService _adminAuth;
    private readonly IProjectService _projectService;

    public AdminProjectPermissionController(IAdminAuthService adminAuth, IProjectService projectService)
    {
        _adminAuth = adminAuth;
        _projectService = projectService;
    }

    /// <summary>授予单个项目权限（需要 MasterKey 验证）</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectPermissionResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> Grant(
        [FromHeader(Name = "X-Master-Key")] string masterKey,
        [FromBody] GrantPermissionRequest request)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        try
        {
            var result = await _projectService.GrantPermissionAsync(request.ClientId, request.ProjectId);
            return Ok(ApiResponse<ProjectPermissionResponse>.Ok(result));
        }
        catch (DuplicateWaitObjectException ex)
        {
            return Conflict(ApiResponse<object>.Fail(409, ex.Message));
        }
    }

    /// <summary>批量授予项目权限</summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectPermissionResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> BatchGrant(
        [FromHeader(Name = "X-Master-Key")] string masterKey,
        [FromBody] BatchGrantPermissionRequest request)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        var results = await _projectService.BatchGrantPermissionAsync(request.ClientId, request.ProjectIds);
        return Ok(ApiResponse<List<ProjectPermissionResponse>>.Ok(results));
    }

    /// <summary>查询权限分配列表（可按 clientId 过滤）</summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectPermissionResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<IActionResult> List(
        [FromHeader(Name = "X-Master-Key")] string masterKey,
        [FromBody] ListPermissionsRequest request)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        var results = await _projectService.ListPermissionsAsync(request.ClientId);
        return Ok(ApiResponse<List<ProjectPermissionResponse>>.Ok(results));
    }

    /// <summary>撤销项目权限</summary>
    [HttpPost("{id}/revoke")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Revoke(
        Guid id,
        [FromHeader(Name = "X-Master-Key")] string masterKey)
    {
        if (!_adminAuth.ValidateMasterKey(masterKey))
            return Unauthorized(ApiResponse<object>.Fail(401, "Master Key 无效"));

        var success = await _projectService.RevokePermissionAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "权限记录不存在"));

        return Ok(ApiResponse<object>.Ok(new { }, "权限已撤销"));
    }
}
