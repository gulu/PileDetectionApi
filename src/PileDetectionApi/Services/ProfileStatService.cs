using AutoMapper;
using FreeSql;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class ProfileStatService : IProfileStatService
{
    private readonly IFreeSql _fsql;
    private readonly IMapper _mapper;
    private readonly ILogger<ProfileStatService> _logger;

    public ProfileStatService(IFreeSql fsql, IMapper mapper, ILogger<ProfileStatService> logger)
    {
        _fsql = fsql;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<ProfileStatResponse>> CreateBatchAsync(Guid pileInfoId, List<CreateProfileStatRequest> requests)
    {
        var exists = await _fsql.Select<PileInfoEntity>().Where(p => p.Id == pileInfoId && !p.IsDeleted).AnyAsync();
        if (!exists) throw new KeyNotFoundException($"基桩不存在: Id={pileInfoId}");

        var entities = requests.Select(r =>
        {
            var e = _mapper.Map<ProfileStatEntity>(r);
            e.Id = Guid.NewGuid();
            e.PileInfoId = pileInfoId;
            e.CreatedAt = DateTime.UtcNow;
            return e;
        }).ToList();

        await _fsql.Insert(entities).ExecuteAffrowsAsync();
        _logger.LogInformation("剖面统计批量创建成功: PileInfoId={Id}, Count={Count}", pileInfoId, entities.Count);
        return _mapper.Map<List<ProfileStatResponse>>(entities);
    }

    public async Task<List<ProfileStatResponse>> GetByPileIdAsync(Guid pileInfoId)
    {
        var items = await _fsql.Select<ProfileStatEntity>()
            .Where(m => m.PileInfoId == pileInfoId)
            .OrderBy("Profile asc").ToListAsync();
        return _mapper.Map<List<ProfileStatResponse>>(items);
    }

    public async Task<ProfileStatResponse> UpdateAsync(Guid id, UpdateProfileStatRequest request)
    {
        var entity = await _fsql.Select<ProfileStatEntity>().Where(m => m.Id == id).FirstAsync();
        if (entity == null) throw new KeyNotFoundException($"剖面统计不存在: Id={id}");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _fsql.Update<ProfileStatEntity>().SetSource(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("剖面统计更新成功: Id={Id}", id);
        return _mapper.Map<ProfileStatResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var affected = await _fsql.Delete<ProfileStatEntity>().Where(m => m.Id == id).ExecuteAffrowsAsync();
        if (affected > 0) _logger.LogInformation("剖面统计删除成功: Id={Id}", id);
        return affected > 0;
    }
}
