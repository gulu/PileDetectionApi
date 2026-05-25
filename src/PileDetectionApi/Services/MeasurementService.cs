using AutoMapper;
using FreeSql;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class MeasurementService : IMeasurementService
{
    private readonly IFreeSql _fsql;
    private readonly IMapper _mapper;
    private readonly ILogger<MeasurementService> _logger;

    public MeasurementService(IFreeSql fsql, IMapper mapper, ILogger<MeasurementService> logger)
    {
        _fsql = fsql;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<MeasurementResponse>> CreateBatchAsync(Guid pileInfoId, List<CreateMeasurementRequest> requests)
    {
        var exists = await _fsql.Select<PileInfoEntity>().Where(p => p.Id == pileInfoId && !p.IsDeleted).AnyAsync();
        if (!exists) throw new KeyNotFoundException($"基桩不存在: Id={pileInfoId}");

        var entities = requests.Select(r =>
        {
            var e = _mapper.Map<MeasurementDataEntity>(r);
            e.Id = Guid.NewGuid();
            e.PileInfoId = pileInfoId;
            e.CreatedAt = DateTime.UtcNow;
            return e;
        }).ToList();

        await _fsql.Insert(entities).ExecuteAffrowsAsync();
        _logger.LogInformation("测点数据批量创建成功: PileInfoId={Id}, Count={Count}", pileInfoId, entities.Count);
        return _mapper.Map<List<MeasurementResponse>>(entities);
    }

    public async Task<List<MeasurementResponse>> GetByPileIdAsync(Guid pileInfoId, double? minDepth = null, double? maxDepth = null)
    {
        var query = _fsql.Select<MeasurementDataEntity>()
            .Where(m => m.PileInfoId == pileInfoId);
        if (minDepth.HasValue) query = query.Where(m => m.Depth >= minDepth.Value);
        if (maxDepth.HasValue) query = query.Where(m => m.Depth <= maxDepth.Value);

        var items = await query.OrderBy("Profile asc, Depth asc").ToListAsync();
        return _mapper.Map<List<MeasurementResponse>>(items);
    }

    public async Task<List<MeasurementResponse>> GetByProfileAsync(Guid pileInfoId, string profile)
    {
        var items = await _fsql.Select<MeasurementDataEntity>()
            .Where(m => m.PileInfoId == pileInfoId && m.Profile == profile)
            .OrderBy("Depth asc").ToListAsync();
        return _mapper.Map<List<MeasurementResponse>>(items);
    }

    public async Task<MeasurementResponse> UpdateAsync(Guid id, UpdateMeasurementRequest request)
    {
        var entity = await _fsql.Select<MeasurementDataEntity>().Where(m => m.Id == id).FirstAsync();
        if (entity == null) throw new KeyNotFoundException($"测点数据不存在: Id={id}");

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _fsql.Update<MeasurementDataEntity>().SetSource(entity).ExecuteAffrowsAsync();
        _logger.LogInformation("测点数据更新成功: Id={Id}", id);
        return _mapper.Map<MeasurementResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var affected = await _fsql.Delete<MeasurementDataEntity>().Where(m => m.Id == id).ExecuteAffrowsAsync();
        if (affected > 0) _logger.LogInformation("测点数据删除成功: Id={Id}", id);
        return affected > 0;
    }
}
