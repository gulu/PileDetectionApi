using System.Text.Json;
using AutoMapper;
using FreeSql;
using Microsoft.Extensions.Logging;
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
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MeasurementService(IFreeSql fsql, IMapper mapper, ILogger<MeasurementService> logger)
    {
        _fsql = fsql;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<MeasurementResponse>> CreateBatchAsync(Guid pileInfoId, List<CreateMeasurementRequest> requests, string clientId)
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

        // 审计日志：记录每条新插入的数据
        foreach (var entity in entities)
        {
            await WriteAuditLogAsync(entity.Id, "INSERT", clientId, null, entity);
        }

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

    public async Task<MeasurementResponse> UpdateAsync(Guid id, UpdateMeasurementRequest request, string clientId)
    {
        var entity = await _fsql.Select<MeasurementDataEntity>().Where(m => m.Id == id).FirstAsync();
        if (entity == null) throw new KeyNotFoundException($"测点数据不存在: Id={id}");

        // 记录修改前的数据快照
        var previousSnapshot = JsonSerializer.Serialize(entity, _jsonOptions);

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _fsql.Update<MeasurementDataEntity>().SetSource(entity).ExecuteAffrowsAsync();

        // 审计日志：记录修改前后数据
        await WriteAuditLogAsync(id, "UPDATE", clientId, previousSnapshot, entity);

        _logger.LogInformation("测点数据更新成功: Id={Id}", id);
        return _mapper.Map<MeasurementResponse>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, string clientId)
    {
        var entity = await _fsql.Select<MeasurementDataEntity>().Where(m => m.Id == id).FirstAsync();
        if (entity == null) return false;

        // 记录删除前的数据快照
        var previousSnapshot = JsonSerializer.Serialize(entity, _jsonOptions);

        var affected = await _fsql.Delete<MeasurementDataEntity>().Where(m => m.Id == id).ExecuteAffrowsAsync();

        // 审计日志：记录删除前数据（无新数据）
        await WriteAuditLogAsync(id, "DELETE", clientId, previousSnapshot, null);

        if (affected > 0) _logger.LogInformation("测点数据删除成功: Id={Id}", id);
        return affected > 0;
    }

    private async Task WriteAuditLogAsync(Guid measurementId, string operationType, string clientId, string? previousData, object? newEntity)
    {
        try
        {
            var log = new MeasurementAuditLogEntity
            {
                Id = Guid.NewGuid(),
                MeasurementId = measurementId,
                OperationType = operationType,
                ClientId = clientId,
                PreviousData = previousData,
                NewData = newEntity != null ? JsonSerializer.Serialize(newEntity, _jsonOptions) : null,
                CreatedAt = DateTime.UtcNow
            };
            await _fsql.Insert(log).ExecuteAffrowsAsync();
        }
        catch (Exception ex)
        {
            // 审计日志写入失败不应影响主业务操作，记录警告即可
            _logger.LogWarning(ex, "审计日志写入失败: MeasurementId={Id}, Operation={Op}", measurementId, operationType);
        }
    }
}
