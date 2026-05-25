using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Authorize]
public class PileController : ControllerBase
{
    private readonly IPileService _pileService;
    private readonly IExportService _exportService;

    public PileController(IPileService pileService, IExportService exportService)
    {
        _pileService = pileService;
        _exportService = exportService;
    }

    /// <summary>在项目下创建基桩</summary>
    [HttpPost("api/v1/projects/{projectId}/piles")]
    [ProducesResponseType(typeof(ApiResponse<PileResponse>), 201)]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreatePileRequest request)
    {
        var result = await _pileService.CreateAsync(projectId, request);
        return Created("", ApiResponse<PileResponse>.Created(result));
    }

    /// <summary>查询项目下所有基桩列表</summary>
    [HttpPost("api/v1/projects/{projectId}/piles/list")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<PileSummaryResponse>>), 200)]
    public async Task<IActionResult> GetByProject(Guid projectId, [FromBody] PagedQueryRequest request)
    {
        var result = await _pileService.GetByProjectIdAsync(projectId, request.Page, request.PageSize);
        return Ok(ApiResponse<PagedResponse<PileSummaryResponse>>.Ok(result));
    }

    /// <summary>查询单桩完整信息</summary>
    [HttpPost("api/v1/piles/{id}")]
    [ProducesResponseType(typeof(ApiResponse<PileDetailResponse>), 200)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _pileService.GetDetailAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail(404, "基桩不存在"));
        return Ok(ApiResponse<PileDetailResponse>.Ok(result));
    }

    /// <summary>更新基桩信息</summary>
    [HttpPost("api/v1/piles/{id}/update")]
    [ProducesResponseType(typeof(ApiResponse<PileResponse>), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePileRequest request)
    {
        var result = await _pileService.UpdateAsync(id, request);
        return Ok(ApiResponse<PileResponse>.Ok(result));
    }

    /// <summary>软删除基桩</summary>
    [HttpPost("api/v1/piles/{id}/delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _pileService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "基桩不存在"));
        return Ok(ApiResponse<object>.Ok(new { }, "删除成功"));
    }

    /// <summary>导出单桩 xlsx 报告（格式参照 pile1.xlsx）</summary>
    [HttpPost("api/v1/piles/{id}/export")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> Export(Guid id)
    {
        var data = await _exportService.ExportPileReportAsync(id);
        var pile = await _pileService.GetByIdAsync(id);
        var fileName = $"基桩报告_{pile?.PileName ?? id.ToString()}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
