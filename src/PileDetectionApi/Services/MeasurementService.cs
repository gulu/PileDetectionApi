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

        // 批量写入波形矩阵（仅请求中包含 RawWaveform 的测点）
        var waveformEntities = new List<MeasurementRawWaveformEntity>();
        for (int i = 0; i < entities.Count; i++)
        {
            var request = requests[i];
            if (request.RawWaveform != null)
            {
                var wf = _mapper.Map<MeasurementRawWaveformEntity>(request.RawWaveform);
                wf.MeasurementDataId = entities[i].Id;
                wf.PileInfoId = pileInfoId;
                wf.CreatedAt = DateTime.UtcNow;
                waveformEntities.Add(wf);
            }
        }
        if (waveformEntities.Count > 0)
        {
            await _fsql.Insert(waveformEntities).ExecuteAffrowsAsync();
        }

        // 审计日志：记录每条新插入的数据
        foreach (var entity in entities)
        {
            await WriteAuditLogAsync(entity.Id, "INSERT", clientId, null, entity);
        }

        _logger.LogInformation("测点数据批量创建成功: PileInfoId={Id}, Count={Count}, WaveformCount={WfCount}",
            pileInfoId, entities.Count, waveformEntities.Count);

        // 构建响应，标记 HasWaveform
        var waveformIds = waveformEntities.Select(w => w.MeasurementDataId).ToHashSet();
        var responses = _mapper.Map<List<MeasurementResponse>>(entities);
        foreach (var resp in responses)
        {
            resp.HasWaveform = waveformIds.Contains(resp.Id);
        }
        return responses;
    }

    public async Task<List<MeasurementResponse>> GetByPileIdAsync(Guid pileInfoId, double? minDepth = null, double? maxDepth = null)
    {
        var query = _fsql.Select<MeasurementDataEntity>()
            .Where(m => m.PileInfoId == pileInfoId);
        if (minDepth.HasValue) query = query.Where(m => m.Depth >= minDepth.Value);
        if (maxDepth.HasValue) query = query.Where(m => m.Depth <= maxDepth.Value);

        var items = await query.OrderBy("Profile asc, Depth asc").ToListAsync();
        var responses = _mapper.Map<List<MeasurementResponse>>(items);

        // 批量查询哪些测点有波形数据
        await PopulateHasWaveformAsync(responses);

        return responses;
    }

    public async Task<List<MeasurementResponse>> GetByProfileAsync(Guid pileInfoId, string profile)
    {
        var items = await _fsql.Select<MeasurementDataEntity>()
            .Where(m => m.PileInfoId == pileInfoId && m.Profile == profile)
            .OrderBy("Depth asc").ToListAsync();
        var responses = _mapper.Map<List<MeasurementResponse>>(items);

        await PopulateHasWaveformAsync(responses);

        return responses;
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

        // 处理波形矩阵（可选）
        bool hasWaveform = false;
        if (request.RawWaveform != null)
        {
            // 先删后插，实现 upsert
            await _fsql.Delete<MeasurementRawWaveformEntity>()
                .Where(w => w.MeasurementDataId == id).ExecuteAffrowsAsync();

            var wf = _mapper.Map<MeasurementRawWaveformEntity>(request.RawWaveform);
            wf.MeasurementDataId = id;
            wf.PileInfoId = entity.PileInfoId;
            wf.CreatedAt = DateTime.UtcNow;
            await _fsql.Insert(wf).ExecuteAffrowsAsync();
            hasWaveform = true;
        }

        // 审计日志：记录修改前后数据
        await WriteAuditLogAsync(id, "UPDATE", clientId, previousSnapshot, entity);

        _logger.LogInformation("测点数据更新成功: Id={Id}, HasWaveform={HasWf}", id, hasWaveform);

        var response = _mapper.Map<MeasurementResponse>(entity);
        response.HasWaveform = hasWaveform || await HasExistingWaveformAsync(id);
        return response;
    }

    public async Task<bool> DeleteAsync(Guid id, string clientId)
    {
        var entity = await _fsql.Select<MeasurementDataEntity>().Where(m => m.Id == id).FirstAsync();
        if (entity == null) return false;

        // 记录删除前的数据快照
        var previousSnapshot = JsonSerializer.Serialize(entity, _jsonOptions);

        // 级联删除波形矩阵
        await _fsql.Delete<MeasurementRawWaveformEntity>()
            .Where(w => w.MeasurementDataId == id).ExecuteAffrowsAsync();

        var affected = await _fsql.Delete<MeasurementDataEntity>().Where(m => m.Id == id).ExecuteAffrowsAsync();

        // 审计日志：记录删除前数据（无新数据）
        await WriteAuditLogAsync(id, "DELETE", clientId, previousSnapshot, null);

        if (affected > 0) _logger.LogInformation("测点数据删除成功: Id={Id}", id);
        return affected > 0;
    }

    public async Task<MeasurementWaveformResponse?> GetWaveformAsync(Guid measurementDataId)
    {
        var entity = await _fsql.Select<MeasurementRawWaveformEntity>()
            .Where(w => w.MeasurementDataId == measurementDataId)
            .FirstAsync();

        if (entity == null) return null;
        return _mapper.Map<MeasurementWaveformResponse>(entity);
    }

    /// <summary>批量填充 HasWaveform 字段</summary>
    private async Task PopulateHasWaveformAsync(List<MeasurementResponse> responses)
    {
        if (responses.Count == 0) return;

        var ids = responses.Select(r => r.Id).ToList();
        var existingIds = await _fsql.Select<MeasurementRawWaveformEntity>()
            .Where(w => ids.Contains(w.MeasurementDataId))
            .ToListAsync(w => w.MeasurementDataId);

        var idSet = existingIds.ToHashSet();
        foreach (var resp in responses)
        {
            resp.HasWaveform = idSet.Contains(resp.Id);
        }
    }

    /// <summary>检查指定测点是否已有波形数据（不加载内容）</summary>
    private async Task<bool> HasExistingWaveformAsync(Guid measurementDataId)
    {
        return await _fsql.Select<MeasurementRawWaveformEntity>()
            .Where(w => w.MeasurementDataId == measurementDataId)
            .AnyAsync();
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
