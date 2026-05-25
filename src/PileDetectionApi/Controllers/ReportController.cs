using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // ===== 单桩报告 =====

    /// <summary>创建/更新单桩报告（POST 幂等，自动判断创建或更新）</summary>
    [HttpPost("api/v1/piles/{pileId}/report")]
    [ProducesResponseType(typeof(ApiResponse<PileReportResponse>), 200)]
    public async Task<IActionResult> CreateOrUpdatePileReport(Guid pileId, [FromBody] CreatePileReportRequest request)
    {
        var result = await _reportService.CreateOrUpdatePileReportAsync(pileId, request);
        return Ok(ApiResponse<PileReportResponse>.Ok(result));
    }

    /// <summary>查询单桩报告</summary>
    [HttpPost("api/v1/piles/{pileId}/report/detail")]
    [ProducesResponseType(typeof(ApiResponse<PileReportResponse>), 200)]
    public async Task<IActionResult> GetPileReport(Guid pileId)
    {
        var result = await _reportService.GetPileReportAsync(pileId);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail(404, "单桩报告不存在"));
        return Ok(ApiResponse<PileReportResponse>.Ok(result));
    }

    // ===== 项目报告 =====

    /// <summary>创建/更新项目报告</summary>
    [HttpPost("api/v1/projects/{projectId}/report")]
    [ProducesResponseType(typeof(ApiResponse<ProjectReportResponse>), 200)]
    public async Task<IActionResult> CreateOrUpdateProjectReport(Guid projectId, [FromBody] CreateProjectReportRequest request)
    {
        var result = await _reportService.CreateOrUpdateProjectReportAsync(projectId, request);
        return Ok(ApiResponse<ProjectReportResponse>.Ok(result));
    }

    /// <summary>查询项目报告</summary>
    [HttpPost("api/v1/projects/{projectId}/report/detail")]
    [ProducesResponseType(typeof(ApiResponse<ProjectReportResponse>), 200)]
    public async Task<IActionResult> GetProjectReport(Guid projectId)
    {
        var result = await _reportService.GetProjectReportAsync(projectId);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail(404, "项目报告不存在"));
        return Ok(ApiResponse<ProjectReportResponse>.Ok(result));
    }
}
