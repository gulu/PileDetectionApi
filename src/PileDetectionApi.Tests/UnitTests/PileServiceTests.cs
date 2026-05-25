using AutoMapper;
using FreeSql;
using Microsoft.Extensions.Logging;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.Entities;
using PileDetectionApi.Mappings;
using PileDetectionApi.Services;
using Xunit;

namespace PileDetectionApi.Tests.UnitTests;

public class PileServiceTests : IDisposable
{
    private readonly IFreeSql _fsql;
    private readonly PileService _service;
    private readonly IMapper _mapper;
    private Guid _projectId;

    public PileServiceTests()
    {
        _fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
            .Build();

        _fsql.CodeFirst.SyncStructure(
            typeof(ProjectInfoEntity), typeof(PileInfoEntity),
            typeof(ProfileStatEntity), typeof(MeasurementDataEntity),
            typeof(PileReportEntity), typeof(ProjectReportEntity),
            typeof(ApiLogEntity));

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        // 创建测试项目
        _projectId = Guid.NewGuid();
        _fsql.Insert(new ProjectInfoEntity
        {
            Id = _projectId,
            ProjectName = "测试项目",
            CreatedAt = DateTime.UtcNow,
            ApiVersion = "v1"
        }).ExecuteAffrows();

        var logger = new LoggerFactory().CreateLogger<PileService>();
        _service = new PileService(_fsql, _mapper, logger);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePileUnderProject()
    {
        var request = new CreatePileRequest
        {
            PileName = "右11",
            DesignLength = 11.7,
            DesignDiameter = 2000,
            DesignStrength = "C25"
        };

        var result = await _service.CreateAsync(_projectId, request);

        Assert.NotNull(result);
        Assert.Equal("右11", result.PileName);
        Assert.Equal(11.7, result.DesignLength);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProjectNotExists()
    {
        var request = new CreatePileRequest { PileName = "测试桩" };
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        var request = new CreatePileRequest { PileName = "待删除" };
        var created = await _service.CreateAsync(_projectId, request);

        await _service.DeleteAsync(created.Id);
        var fetched = await _service.GetByIdAsync(created.Id);

        Assert.Null(fetched);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePileFields()
    {
        var request = new CreatePileRequest { PileName = "旧名称" };
        var created = await _service.CreateAsync(_projectId, request);

        var update = new UpdatePileRequest { PileName = "新名称", DesignLength = 15.0 };
        var updated = await _service.UpdateAsync(created.Id, update);

        Assert.Equal("新名称", updated.PileName);
        Assert.Equal(15.0, updated.DesignLength);
    }

    [Fact]
    public async Task GetByProjectIdAsync_ShouldReturnPagedPiles()
    {
        for (int i = 1; i <= 3; i++)
            await _service.CreateAsync(_projectId, new CreatePileRequest { PileName = $"桩{i}" });

        var result = await _service.GetByProjectIdAsync(_projectId, 1, 2);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    public void Dispose()
    {
        _fsql.Dispose();
    }
}
