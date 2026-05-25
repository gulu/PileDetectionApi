using AutoMapper;
using FreeSql;
using Microsoft.Extensions.Logging;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class ProjectService : IProjectService
{
    private readonly IFreeSql _fsql;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IFreeSql fsql, IMapper mapper, ILogger<ProjectService> logger)
    {
        _fsql = fsql;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request)
    {
        var entity = _mapper.Map<ProjectInfoEntity>(request);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;

        await _fsql.Insert(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("项目创建成功: Id={Id}, Name={Name}", entity.Id, request.ProjectName);
        return _mapper.Map<ProjectResponse>(entity);
    }

    public async Task<ProjectResponse?> GetByIdAsync(Guid id)
    {
        var entity = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => p.Id == id && !p.IsDeleted).FirstAsync();
        return entity == null ? null : _mapper.Map<ProjectResponse>(entity);
    }

    public async Task<ProjectDetailResponse?> GetDetailAsync(Guid id)
    {
        var entity = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => p.Id == id && !p.IsDeleted).FirstAsync();
        if (entity == null) return null;

        var pileCount = await _fsql.Select<PileInfoEntity>()
            .Where(p => p.ProjectId == id && !p.IsDeleted).CountAsync();

        var response = _mapper.Map<ProjectDetailResponse>(entity);
        response.PileCount = (int)pileCount;
        return response;
    }

    public async Task<PagedResponse<ProjectResponse>> GetPagedAsync(int page, int pageSize, string? keyword)
    {
        var query = _fsql.Select<ProjectInfoEntity>()
            .Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.ProjectName.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        return new PagedResponse<ProjectResponse>
        {
            Items = _mapper.Map<List<ProjectResponse>>(items),
            Page = page, PageSize = pageSize, TotalCount = (int)total
        };
    }

    public async Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request)
    {
        var entity = await _fsql.Select<ProjectInfoEntity>().Where(p => p.Id == id).FirstAsync();
        if (entity == null) throw new KeyNotFoundException($"项目不存在: Id={id}");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _fsql.Update<ProjectInfoEntity>().SetSource(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("项目更新成功: Id={Id}", id);
        return _mapper.Map<ProjectResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var affected = await _fsql.Update<ProjectInfoEntity>()
            .Set(p => p.IsDeleted, true)
            .Set(p => p.UpdatedAt, DateTime.UtcNow)
            .Where(p => p.Id == id && !p.IsDeleted)
            .ExecuteAffrowsAsync();
        if (affected > 0) _logger.LogInformation("项目软删除成功: Id={Id}", id);
        return affected > 0;
    }

    public async Task<int> GetPileCountAsync(Guid projectId)
    {
        return (int)await _fsql.Select<PileInfoEntity>()
            .Where(p => p.ProjectId == projectId && !p.IsDeleted).CountAsync();
    }
}
