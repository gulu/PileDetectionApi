using AutoMapper;
using FreeSql;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class ReportService : IReportService
{
    private readonly IFreeSql _fsql;
    private readonly IMapper _mapper;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IFreeSql fsql, IMapper mapper, ILogger<ReportService> logger)
    {
        _fsql = fsql;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PileReportResponse> CreateOrUpdatePileReportAsync(Guid pileInfoId, CreatePileReportRequest request)
    {
        var pileExists = await _fsql.Select<PileInfoEntity>()
            .Where(p => p.Id == pileInfoId && !p.IsDeleted).AnyAsync();
        if (!pileExists)
            throw new KeyNotFoundException($"基桩不存在: Id={pileInfoId}");

        // 检查是否已存在，存在则更新
        var existing = await _fsql.Select<PileReportEntity>()
            .Where(r => r.PileInfoId == pileInfoId).FirstAsync();

        if (existing != null)
        {
            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            await _fsql.Update<PileReportEntity>().SetSource(existing).ExecuteAffrowsAsync();
            _logger.LogInformation("单桩报告更新成功: PileId={PileId}", pileInfoId);

            if (request.IntegrityCategory.HasValue)
            {
                await _fsql.Update<PileInfoEntity>()
                    .Set(p => p.IntegrityCategory, request.IntegrityCategory.Value)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow)
                    .Where(p => p.Id == pileInfoId)
                    .ExecuteAffrowsAsync();
            }
            return _mapper.Map<PileReportResponse>(existing);
        }

        var entity = _mapper.Map<PileReportEntity>(request);
        entity.Id = Guid.NewGuid();
        entity.PileInfoId = pileInfoId;
        entity.CreatedAt = DateTime.UtcNow;
        await _fsql.Insert(entity).ExecuteAffrowsAsync();

        _logger.LogInformation("单桩报告创建成功: PileId={PileId}", pileInfoId);

        if (request.IntegrityCategory.HasValue)
        {
            await _fsql.Update<PileInfoEntity>()
                .Set(p => p.IntegrityCategory, request.IntegrityCategory.Value)
                .Set(p => p.UpdatedAt, DateTime.UtcNow)
                .Where(p => p.Id == pileInfoId)
                .ExecuteAffrowsAsync();
        }
        return _mapper.Map<PileReportResponse>(entity);
    }

    public async Task<PileReportResponse?> GetPileReportAsync(Guid pileInfoId)
    {
        var entity = await _fsql.Select<PileReportEntity>()
            .Where(r => r.PileInfoId == pileInfoId).FirstAsync();
        return entity == null ? null : _mapper.Map<PileReportResponse>(entity);
    }

    public async Task<ProjectReportResponse> CreateOrUpdateProjectReportAsync(Guid projectId, CreateProjectReportRequest request)
    {
        var projectExists = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => p.Id == projectId && !p.IsDeleted).AnyAsync();
        if (!projectExists)
            throw new KeyNotFoundException($"项目不存在: Id={projectId}");

        var existing = await _fsql.Select<ProjectReportEntity>()
            .Where(r => r.ProjectId == projectId).FirstAsync();

        if (existing != null)
        {
            _mapper.Map(request, existing);
            existing.UpdatedAt = DateTime.UtcNow;
            await _fsql.Update<ProjectReportEntity>().SetSource(existing).ExecuteAffrowsAsync();
            _logger.LogInformation("项目报告更新成功: ProjectId={ProjectId}", projectId);
            return _mapper.Map<ProjectReportResponse>(existing);
        }

        var entity = _mapper.Map<ProjectReportEntity>(request);
        entity.Id = Guid.NewGuid();
        entity.ProjectId = projectId;
        entity.CreatedAt = DateTime.UtcNow;
        await _fsql.Insert(entity).ExecuteAffrowsAsync();

        _logger.LogInformation("项目报告创建成功: ProjectId={ProjectId}", projectId);
        return _mapper.Map<ProjectReportResponse>(entity);
    }

    public async Task<ProjectReportResponse?> GetProjectReportAsync(Guid projectId)
    {
        var entity = await _fsql.Select<ProjectReportEntity>()
            .Where(r => r.ProjectId == projectId).FirstAsync();
        return entity == null ? null : _mapper.Map<ProjectReportResponse>(entity);
    }
}
