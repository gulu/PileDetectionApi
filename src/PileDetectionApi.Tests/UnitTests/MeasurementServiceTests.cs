using System.Text.Json;
using AutoMapper;
using FreeSql;
using Microsoft.Extensions.Logging;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.Entities;
using PileDetectionApi.Mappings;
using PileDetectionApi.Services;
using Xunit;

namespace PileDetectionApi.Tests.UnitTests;

/// <summary>
/// 测点数据服务单元测试 —— 含审计日志验证
/// </summary>
public class MeasurementServiceTests : IDisposable
{
    private readonly IFreeSql _fsql;
    private readonly MeasurementService _service;
    private readonly IMapper _mapper;
    private Guid _pileInfoId;
    private const string TestClientId = "test-client";

    public MeasurementServiceTests()
    {
        _fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
            .Build();

        _fsql.CodeFirst.SyncStructure(
            typeof(ProjectInfoEntity), typeof(PileInfoEntity),
            typeof(MeasurementDataEntity), typeof(MeasurementAuditLogEntity),
            typeof(MeasurementRawWaveformEntity));

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        // 创建测试项目和测试桩
        var projectId = Guid.NewGuid();
        _fsql.Insert(new ProjectInfoEntity
        {
            Id = projectId,
            ProjectName = "审计测试项目",
            CreatedAt = DateTime.UtcNow,
            ApiVersion = "v1"
        }).ExecuteAffrows();

        _pileInfoId = Guid.NewGuid();
        _fsql.Insert(new PileInfoEntity
        {
            Id = _pileInfoId,
            ProjectId = projectId,
            PileName = "测试桩-1",
            CreatedAt = DateTime.UtcNow,
            ApiVersion = "v1"
        }).ExecuteAffrows();

        var logger = new LoggerFactory().CreateLogger<MeasurementService>();
        _service = new MeasurementService(_fsql, _mapper, logger);
    }

    [Fact]
    public async Task CreateBatchAsync_ShouldCreateMeasurementData()
    {
        var requests = new List<CreateMeasurementRequest>
        {
            new() { Profile = "AB", Depth = 1.0, SoundVelocity = 4000, Amplitude = 80 },
            new() { Profile = "AB", Depth = 2.0, SoundVelocity = 4100, Amplitude = 85 }
        };

        var results = await _service.CreateBatchAsync(_pileInfoId, requests, TestClientId);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.Equal(_pileInfoId, r.PileInfoId));
    }

    [Fact]
    public async Task CreateBatchAsync_ShouldThrow_WhenPileNotExists()
    {
        var request = new List<CreateMeasurementRequest>
        {
            new() { Profile = "AB", Depth = 1.0 }
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.CreateBatchAsync(Guid.NewGuid(), request, TestClientId));
    }

    [Fact]
    public async Task CreateBatchAsync_ShouldWriteAuditLog()
    {
        var requests = new List<CreateMeasurementRequest>
        {
            new() { Profile = "CD", Depth = 1.5, SoundVelocity = 3800, Amplitude = 75 }
        };

        await _service.CreateBatchAsync(_pileInfoId, requests, TestClientId);

        var logs = await _fsql.Select<MeasurementAuditLogEntity>().ToListAsync();
        Assert.Single(logs);
        Assert.Equal("INSERT", logs[0].OperationType);
        Assert.Equal(TestClientId, logs[0].ClientId);
        Assert.Null(logs[0].PreviousData);
        Assert.NotNull(logs[0].NewData);

        // 验证 NewData 包含正确的内容
        var newData = JsonSerializer.Deserialize<JsonElement>(logs[0].NewData!);
        Assert.Equal("CD", newData.GetProperty("profile").GetString());
        Assert.Equal(3800, newData.GetProperty("soundVelocity").GetDouble());
    }

    [Fact]
    public async Task UpdateAsync_ShouldWriteAuditLogWithBeforeAndAfter()
    {
        // 先创建一条数据
        var requests = new List<CreateMeasurementRequest>
        {
            new() { Profile = "EF", Depth = 1.0, SoundVelocity = 3500, Amplitude = 70 }
        };
        var created = await _service.CreateBatchAsync(_pileInfoId, requests, TestClientId);
        var measurementId = created[0].Id;

        // 更新
        var update = new UpdateMeasurementRequest { SoundVelocity = 4200, Amplitude = 90 };
        var updated = await _service.UpdateAsync(measurementId, update, TestClientId);

        Assert.Equal(4200, updated.SoundVelocity);
        Assert.Equal(90, updated.Amplitude);

        // 验证审计日志：应有 1 条 INSERT + 1 条 UPDATE
        var logs = await _fsql.Select<MeasurementAuditLogEntity>()
            .OrderBy(l => l.CreatedAt).ToListAsync();

        Assert.Equal(2, logs.Count);

        // 第 2 条是 UPDATE
        var updateLog = logs[1];
        Assert.Equal("UPDATE", updateLog.OperationType);
        Assert.Equal(TestClientId, updateLog.ClientId);
        Assert.NotNull(updateLog.PreviousData);
        Assert.NotNull(updateLog.NewData);

        var prevData = JsonSerializer.Deserialize<JsonElement>(updateLog.PreviousData!);
        Assert.Equal(3500, prevData.GetProperty("soundVelocity").GetDouble());

        var newData = JsonSerializer.Deserialize<JsonElement>(updateLog.NewData!);
        Assert.Equal(4200, newData.GetProperty("soundVelocity").GetDouble());
    }

    [Fact]
    public async Task DeleteAsync_ShouldWriteAuditLogWithPreviousData()
    {
        // 先创建一条数据
        var requests = new List<CreateMeasurementRequest>
        {
            new() { Profile = "GH", Depth = 3.0, SoundVelocity = 3900 }
        };
        var created = await _service.CreateBatchAsync(_pileInfoId, requests, TestClientId);
        var measurementId = created[0].Id;

        // 删除
        var deleted = await _service.DeleteAsync(measurementId, TestClientId);
        Assert.True(deleted);

        // 验证审计日志
        var logs = await _fsql.Select<MeasurementAuditLogEntity>()
            .OrderBy(l => l.CreatedAt).ToListAsync();

        Assert.Equal(2, logs.Count);

        var deleteLog = logs[1];
        Assert.Equal("DELETE", deleteLog.OperationType);
        Assert.Equal(TestClientId, deleteLog.ClientId);
        Assert.NotNull(deleteLog.PreviousData);
        Assert.Null(deleteLog.NewData);

        // 验证 PreviousData 包含删除前的数据
        var prevData = JsonSerializer.Deserialize<JsonElement>(deleteLog.PreviousData!);
        Assert.Equal("GH", prevData.GetProperty("profile").GetString());
        Assert.Equal(3900, prevData.GetProperty("soundVelocity").GetDouble());
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotExists()
    {
        var result = await _service.DeleteAsync(Guid.NewGuid(), TestClientId);
        Assert.False(result);
    }

    public void Dispose()
    {
        _fsql.Dispose();
    }
}
