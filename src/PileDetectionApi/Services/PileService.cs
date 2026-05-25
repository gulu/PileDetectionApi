using AutoMapper;
using FreeSql;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class PileService : IPileService
{
    private readonly IFreeSql _fsql;
    private readonly IMapper _mapper;
    private readonly ILogger<PileService> _logger;

    public PileService(IFreeSql fsql, IMapper mapper, ILogger<PileService> logger)
    {
        _fsql = fsql;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PileResponse> CreateAsync(Guid projectId, CreatePileRequest request)
    {
        var projectExists = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => p.Id == projectId && !p.IsDeleted).AnyAsync();
        if (!projectExists)
            throw new KeyNotFoundException($"项目不存在: Id={projectId}");

        var entity = _mapper.Map<PileInfoEntity>(request);
        entity.Id = Guid.NewGuid();
        entity.ProjectId = projectId;
        entity.CreatedAt = DateTime.UtcNow;

        await _fsql.Insert(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("基桩创建成功: Id={Id}, Name={Name}", entity.Id, request.PileName);
        return _mapper.Map<PileResponse>(entity);
    }

    public async Task<PileResponse?> GetByIdAsync(Guid id)
    {
        var entity = await _fsql.Select<PileInfoEntity>()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Include(p => p.Project).FirstAsync();
        if (entity == null) return null;
        var response = _mapper.Map<PileResponse>(entity);
        response.ProjectName = entity.Project?.ProjectName ?? "";
        return response;
    }

    public async Task<PileDetailResponse?> GetDetailAsync(Guid id)
    {
        var entity = await _fsql.Select<PileInfoEntity>()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Include(p => p.Project).FirstAsync();
        if (entity == null) return null;

        entity.ProfileStats = await _fsql.Select<ProfileStatEntity>()
            .Where(m => m.PileInfoId == id).OrderBy("Profile asc").ToListAsync();
        entity.Measurements = await _fsql.Select<MeasurementDataEntity>()
            .Where(m => m.PileInfoId == id).OrderBy("Profile asc, Depth asc").ToListAsync();
        entity.Report = await _fsql.Select<PileReportEntity>()
            .Where(m => m.PileInfoId == id).FirstAsync();

        var response = _mapper.Map<PileDetailResponse>(entity);
        response.ProjectName = entity.Project?.ProjectName ?? "";
        return response;
    }

    public async Task<PagedResponse<PileSummaryResponse>> GetByProjectIdAsync(Guid projectId, int page, int pageSize)
    {
        var query = _fsql.Select<PileInfoEntity>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted);
        var total = await query.CountAsync();
        var items = await query.OrderBy(p => p.PileName)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return new PagedResponse<PileSummaryResponse>
        {
            Items = _mapper.Map<List<PileSummaryResponse>>(items),
            Page = page, PageSize = pageSize, TotalCount = (int)total
        };
    }

    public async Task<PileResponse> UpdateAsync(Guid id, UpdatePileRequest request)
    {
        var entity = await _fsql.Select<PileInfoEntity>().Where(p => p.Id == id && !p.IsDeleted).FirstAsync();
        if (entity == null) throw new KeyNotFoundException($"基桩不存在: Id={id}");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _fsql.Update<PileInfoEntity>().SetSource(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("基桩更新成功: Id={Id}", id);
        return _mapper.Map<PileResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var affected = await _fsql.Update<PileInfoEntity>()
            .Set(p => p.IsDeleted, true)
            .Set(p => p.UpdatedAt, DateTime.UtcNow)
            .Where(p => p.Id == id && !p.IsDeleted)
            .ExecuteAffrowsAsync();
        if (affected > 0) _logger.LogInformation("基桩软删除成功: Id={Id}", id);
        return affected > 0;
    }
}
