using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Route("api/v1/piles/{pileId}/measurements")]
[Authorize]
public class MeasurementController : ControllerBase
{
    private readonly IMeasurementService _measurementService;

    public MeasurementController(IMeasurementService measurementService)
    {
        _measurementService = measurementService;
    }

    /// <summary>批量添加测点数据</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<List<MeasurementResponse>>), 201)]
    public async Task<IActionResult> CreateBatch(Guid pileId, [FromBody] List<CreateMeasurementRequest> requests)
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await _measurementService.CreateBatchAsync(pileId, requests, clientId);
        return Created("", ApiResponse<List<MeasurementResponse>>.Created(result));
    }

    /// <summary>查询所有测点（支持深度范围筛选）</summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(ApiResponse<List<MeasurementResponse>>), 200)]
    public async Task<IActionResult> GetByPile(Guid pileId, [FromBody] MeasurementQueryRequest request)
    {
        var result = await _measurementService.GetByPileIdAsync(pileId, request.MinDepth, request.MaxDepth);
        return Ok(ApiResponse<List<MeasurementResponse>>.Ok(result));
    }

    /// <summary>按剖面查询测点</summary>
    [HttpPost("profile/{profile}")]
    [ProducesResponseType(typeof(ApiResponse<List<MeasurementResponse>>), 200)]
    public async Task<IActionResult> GetByProfile(Guid pileId, string profile)
    {
        var result = await _measurementService.GetByProfileAsync(pileId, profile);
        return Ok(ApiResponse<List<MeasurementResponse>>.Ok(result));
    }

    /// <summary>更新单条测点数据</summary>
    [HttpPost("{id}/update")]
    [ProducesResponseType(typeof(ApiResponse<MeasurementResponse>), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMeasurementRequest request)
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var result = await _measurementService.UpdateAsync(id, request, clientId);
        return Ok(ApiResponse<MeasurementResponse>.Ok(result));
    }

    /// <summary>删除单条测点数据</summary>
    [HttpPost("{id}/delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var clientId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
        var success = await _measurementService.DeleteAsync(id, clientId);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "测点数据不存在"));
        return Ok(ApiResponse<object>.Ok(new { }, "删除成功"));
    }

    /// <summary>查询单条测点的原始波形矩阵数据</summary>
    [HttpPost("{id}/waveform")]
    [ProducesResponseType(typeof(ApiResponse<MeasurementWaveformResponse>), 200)]
    public async Task<IActionResult> GetWaveform(Guid id)
    {
        var result = await _measurementService.GetWaveformAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail(404, "该测点无原始波形矩阵数据"));
        return Ok(ApiResponse<MeasurementWaveformResponse>.Ok(result));
    }
}
