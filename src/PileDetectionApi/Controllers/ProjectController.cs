using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Controllers;

[ApiController]
[Route("api/v1/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IExportService _exportService;

    public ProjectController(IProjectService projectService, IExportService exportService)
    {
        _projectService = projectService;
        _exportService = exportService;
    }

    /// <summary>创建项目</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 201)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var result = await _projectService.CreateAsync(request);
        return Created("", ApiResponse<ProjectResponse>.Created(result));
    }

    /// <summary>分页查询项目列表</summary>
    [HttpPost("list")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<ProjectResponse>>), 200)]
    public async Task<IActionResult> GetPaged([FromBody] PagedQueryRequest request)
    {
        var result = await _projectService.GetPagedAsync(request.Page, request.PageSize, request.Keyword);
        return Ok(ApiResponse<PagedResponse<ProjectResponse>>.Ok(result));
    }

    /// <summary>查询项目详情（含桩数量）</summary>
    [HttpPost("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailResponse>), 200)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _projectService.GetDetailAsync(id);
        if (result == null)
            return NotFound(ApiResponse<object>.Fail(404, "项目不存在"));
        return Ok(ApiResponse<ProjectDetailResponse>.Ok(result));
    }

    /// <summary>更新项目信息</summary>
    [HttpPost("{id}/update")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 200)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var result = await _projectService.UpdateAsync(id, request);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    /// <summary>软删除项目</summary>
    [HttpPost("{id}/delete")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _projectService.DeleteAsync(id);
        if (!success)
            return NotFound(ApiResponse<object>.Fail(404, "项目不存在"));
        return Ok(ApiResponse<object>.Ok(new { }, "删除成功"));
    }

    /// <summary>导出项目 xlsx 汇总报告</summary>
    [HttpPost("{id}/export")]
    [ProducesResponseType(typeof(FileContentResult), 200)]
    public async Task<IActionResult> Export(Guid id)
    {
        var data = await _exportService.ExportProjectReportAsync(id);
        var project = await _projectService.GetByIdAsync(id);
        var fileName = $"项目报告_{project?.ProjectName ?? id.ToString()}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
