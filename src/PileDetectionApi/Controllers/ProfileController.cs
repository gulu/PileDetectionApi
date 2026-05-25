using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Route("api/v1/piles/{pileId}/profiles")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileStatService _profileStatService;

    public ProfileController(IProfileStatService profileStatService)
    {
        _profileStatService = profileStatService;
    }

    /// <summary>批量添加剖面统计</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<List<ProfileStatResponse>>), 201)]
    public async Task<IActionResult> CreateBatch(Guid pileId, [FromBody] List<CreateProfileStatRequest> requests)
    {
        var result = await _profileStatService.CreateBatchAsync(pileId, requests);
        return Created("", ApiResponse<List<ProfileStatResponse>>.Created(result));
    }

    /// <summary>查询桩的所有剖面统计</summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(ApiResponse<List<ProfileStatResponse>>), 200)]
    public async Task<IActionResult> GetByPile(Guid pileId)
    {
        var result = await _profileStatService.GetByPileIdAsync(pileId);
        return Ok(ApiResponse<List<ProfileStatResponse>>.Ok(result));
    }

    /// <summary>更新剖面统计</summary>
    [HttpPost("{id}/update")]
    [ProducesResponseType(typeof(ApiResponse<ProfileStatResponse>), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProfileStatRequest request)
    {
        var result = await _profileStatService.UpdateAsync(id, request);
        return Ok(ApiResponse<ProfileStatResponse>.Ok(result));
    }

    /// <summary>删除剖面统计</summary>
    [HttpPost("{id}/delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _profileStatService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "剖面统计不存在"));
        return Ok(ApiResponse<object>.Ok(new { }, "删除成功"));
    }
}
