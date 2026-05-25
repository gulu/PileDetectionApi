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

    public async Task<PagedResponse<ProjectResponse>> GetPermittedPagedAsync(string clientId, int page, int pageSize, string? keyword)
    {
        var query = _fsql.Select<ProjectInfoEntity>()
            .Where(p => !p.IsDeleted)
            .Where(p => _fsql.Select<ProjectPermissionEntity>()
                .Where(pp => pp.ClientId == clientId && pp.ProjectId == p.Id)
                .Any());

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(p => p.ProjectName.Contains(keyword));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

        _logger.LogInformation("查询客户端权限项目列表: ClientId={ClientId}, Total={Total}, Page={Page}", clientId, total, page);

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

    // ========== 权限管理 ==========

    public async Task<ProjectPermissionResponse> GrantPermissionAsync(string clientId, Guid projectId)
    {
        // 检查是否已存在
        var exists = await _fsql.Select<ProjectPermissionEntity>()
            .Where(pp => pp.ClientId == clientId && pp.ProjectId == projectId)
            .AnyAsync();
        if (exists)
            throw new DuplicateWaitObjectException($"权限已存在: ClientId={clientId}, ProjectId={projectId}");

        var entity = new ProjectPermissionEntity
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow
        };
        await _fsql.Insert(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("授予项目权限: ClientId={ClientId}, ProjectId={ProjectId}", clientId, projectId);

        var projectName = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => p.Id == projectId).FirstAsync(p => p.ProjectName);

        return new ProjectPermissionResponse
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            ProjectId = entity.ProjectId,
            ProjectName = projectName ?? "",
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task<List<ProjectPermissionResponse>> BatchGrantPermissionAsync(string clientId, List<Guid> projectIds)
    {
        var results = new List<ProjectPermissionResponse>();
        var existingIds = await _fsql.Select<ProjectPermissionEntity>()
            .Where(pp => pp.ClientId == clientId && projectIds.Contains(pp.ProjectId))
            .ToListAsync(pp => pp.ProjectId);
        var existingSet = new HashSet<Guid>(existingIds);

        foreach (var projectId in projectIds.Distinct())
        {
            if (existingSet.Contains(projectId)) continue;

            var entity = new ProjectPermissionEntity
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                ProjectId = projectId,
                CreatedAt = DateTime.UtcNow
            };
            await _fsql.Insert(entity).ExecuteAffrowsAsync();

            var projectName = await _fsql.Select<ProjectInfoEntity>()
                .Where(p => p.Id == projectId).FirstAsync(p => p.ProjectName);

            results.Add(new ProjectPermissionResponse
            {
                Id = entity.Id,
                ClientId = entity.ClientId,
                ProjectId = entity.ProjectId,
                ProjectName = projectName ?? "",
                CreatedAt = entity.CreatedAt
            });
        }

        _logger.LogInformation("批量授予项目权限: ClientId={ClientId}, Count={Count}", clientId, results.Count);
        return results;
    }

    public async Task<List<ProjectPermissionResponse>> ListPermissionsAsync(string? clientId)
    {
        var query = _fsql.Select<ProjectPermissionEntity>();

        if (!string.IsNullOrWhiteSpace(clientId))
            query = query.Where(pp => pp.ClientId == clientId);

        var items = await query.OrderByDescending(pp => pp.CreatedAt).ToListAsync();

        // 批量获取关联的项目名称
        var projectIds = items.Select(i => i.ProjectId).Distinct().ToList();
        var projectNames = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => projectIds.Contains(p.Id))
            .ToListAsync(p => new { p.Id, p.ProjectName });
        var nameMap = projectNames.ToDictionary(n => n.Id, n => n.ProjectName);

        return items.Select(item => new ProjectPermissionResponse
        {
            Id = item.Id,
            ClientId = item.ClientId,
            ProjectId = item.ProjectId,
            ProjectName = nameMap.GetValueOrDefault(item.ProjectId) ?? "",
            CreatedAt = item.CreatedAt
        }).ToList();
    }

    public async Task<bool> RevokePermissionAsync(Guid permissionId)
    {
        var affected = await _fsql.Delete<ProjectPermissionEntity>()
            .Where(pp => pp.Id == permissionId)
            .ExecuteAffrowsAsync();
        if (affected > 0)
            _logger.LogInformation("撤销项目权限: Id={Id}", permissionId);
        return affected > 0;
    }
}
