using AutoMapper;
using FreeSql;
using Microsoft.Extensions.Logging;
using PileDetectionApi.Configs;
using PileDetectionApi.Data;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;
using PileDetectionApi.Mappings;
using PileDetectionApi.Services;
using Xunit;

namespace PileDetectionApi.Tests.UnitTests;

/// <summary>
/// 项目服务单元测试 —— 使用 FreeSql 内存 SQLite
/// </summary>
public class ProjectServiceTests : IDisposable
{
    private readonly IFreeSql _fsql;
    private readonly ProjectService _service;
    private readonly IMapper _mapper;

    public ProjectServiceTests()
    {
        // 使用内存 SQLite 作为测试数据库
        _fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
            .Build();

        // 同步实体结构
        _fsql.CodeFirst.SyncStructure(
            typeof(ProjectInfoEntity), typeof(PileInfoEntity),
            typeof(ProfileStatEntity), typeof(MeasurementDataEntity),
            typeof(PileReportEntity), typeof(ProjectReportEntity),
            typeof(ApiLogEntity));

        // AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        var logger = new LoggerFactory().CreateLogger<ProjectService>();
        _service = new ProjectService(_fsql, _mapper, logger);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnProjectResponse()
    {
        var request = new CreateProjectRequest
        {
            ProjectName = "测试项目",
            ProjectNo = "TEST-001",
            ProjectLocation = "测试地点",
            ProjectManager = "测试人"
        };

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("测试项目", result.ProjectName);
        Assert.Equal("TEST-001", result.ProjectNo);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAndGet_ShouldMatch()
    {
        var request = new CreateProjectRequest { ProjectName = "碗窑岭大桥" };
        var created = await _service.CreateAsync(request);

        var fetched = await _service.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("碗窑岭大桥", fetched.ProjectName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyProject()
    {
        var request = new CreateProjectRequest { ProjectName = "原名称" };
        var created = await _service.CreateAsync(request);

        var update = new UpdateProjectRequest { ProjectName = "新名称" };
        var updated = await _service.UpdateAsync(created.Id, update);

        Assert.Equal("新名称", updated.ProjectName);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete()
    {
        var request = new CreateProjectRequest { ProjectName = "待删除项目" };
        var created = await _service.CreateAsync(request);

        var success = await _service.DeleteAsync(created.Id);
        Assert.True(success);

        var fetched = await _service.GetByIdAsync(created.Id);
        Assert.Null(fetched);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // 创建 3 个项目
        for (int i = 1; i <= 3; i++)
            await _service.CreateAsync(new CreateProjectRequest { ProjectName = $"项目{i}" });

        // 不带关键字
        var result = await _service.GetPagedAsync(1, 2, null);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);

        // 带关键字
        var filtered = await _service.GetPagedAsync(1, 10, "项目1");
        Assert.Single(filtered.Items);
    }

    public void Dispose()
    {
        _fsql.Dispose();
    }
}
