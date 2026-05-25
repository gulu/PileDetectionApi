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
            typeof(ApiLogEntity), typeof(ProjectPermissionEntity));

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

    // ========== 权限查询测试 ==========

    [Fact]
    public async Task GetPermittedPagedAsync_ShouldReturnEmpty_WhenNoPermission()
    {
        // 创建一个项目但不插入任何权限记录
        var project = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "无权限项目" });

        var result = await _service.GetPermittedPagedAsync("client-1", 1, 20, null);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetPermittedPagedAsync_ShouldReturnOnlyPermittedProjects()
    {
        // 创建 3 个项目
        var p1 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "项目A" });
        var p2 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "项目B" });
        var p3 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "项目C" });

        // 为 client-1 授予 p1 和 p3 的权限
        _fsql.Insert(new ProjectPermissionEntity
        {
            Id = Guid.NewGuid(), ClientId = "client-1", ProjectId = p1.Id
        }).ExecuteAffrows();
        _fsql.Insert(new ProjectPermissionEntity
        {
            Id = Guid.NewGuid(), ClientId = "client-1", ProjectId = p3.Id
        }).ExecuteAffrows();
        // 为 client-2 授予 p2 的权限
        _fsql.Insert(new ProjectPermissionEntity
        {
            Id = Guid.NewGuid(), ClientId = "client-2", ProjectId = p2.Id
        }).ExecuteAffrows();

        // client-1 应看到项目A和项目C
        var result = await _service.GetPermittedPagedAsync("client-1", 1, 20, null);

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, i => i.ProjectName == "项目A");
        Assert.Contains(result.Items, i => i.ProjectName == "项目C");
        Assert.DoesNotContain(result.Items, i => i.ProjectName == "项目B");
    }

    [Fact]
    public async Task GetPermittedPagedAsync_ShouldRespectPagination()
    {
        // 创建 4 个项目，都授权给 client-1
        for (int i = 1; i <= 4; i++)
        {
            var p = await _service.CreateAsync(new CreateProjectRequest { ProjectName = $"分页项目{i}" });
            _fsql.Insert(new ProjectPermissionEntity
            {
                Id = Guid.NewGuid(), ClientId = "client-1", ProjectId = p.Id
            }).ExecuteAffrows();
        }

        // 第1页，每页2条
        var page1 = await _service.GetPermittedPagedAsync("client-1", 1, 2, null);
        Assert.Equal(4, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);

        // 第2页，每页2条
        var page2 = await _service.GetPermittedPagedAsync("client-1", 2, 2, null);
        Assert.Equal(4, page2.TotalCount);
        Assert.Equal(2, page2.Items.Count);

        // 验证两页数据不重复（按创建时间倒序）
        Assert.NotEqual(page1.Items[0].Id, page2.Items[0].Id);
    }

    [Fact]
    public async Task GetPermittedPagedAsync_ShouldSupportKeywordFilter()
    {
        // 创建项目并授权
        var p1 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "国道大桥" });
        var p2 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "省道大桥" });
        var p3 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "码头工程" });

        foreach (var p in new[] { p1, p2, p3 })
        {
            _fsql.Insert(new ProjectPermissionEntity
            {
                Id = Guid.NewGuid(), ClientId = "client-1", ProjectId = p.Id
            }).ExecuteAffrows();
        }

        // 按关键字 "大桥" 过滤
        var result = await _service.GetPermittedPagedAsync("client-1", 1, 20, "大桥");

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, i => i.ProjectName == "国道大桥");
        Assert.Contains(result.Items, i => i.ProjectName == "省道大桥");
    }

    [Fact]
    public async Task GetPermittedPagedAsync_ShouldReturnEmpty_WhenKeywordNoMatch()
    {
        var p = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "匹配项目" });
        _fsql.Insert(new ProjectPermissionEntity
        {
            Id = Guid.NewGuid(), ClientId = "client-1", ProjectId = p.Id
        }).ExecuteAffrows();

        var result = await _service.GetPermittedPagedAsync("client-1", 1, 20, "不存在的关键字");

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    // ========== 权限管理测试 ==========

    [Fact]
    public async Task GrantPermissionAsync_ShouldCreatePermission()
    {
        var project = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "授权项目" });

        var result = await _service.GrantPermissionAsync("client-1", project.Id);

        Assert.NotNull(result);
        Assert.Equal("client-1", result.ClientId);
        Assert.Equal(project.Id, result.ProjectId);
        Assert.Equal("授权项目", result.ProjectName);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GrantPermissionAsync_ShouldThrow_WhenDuplicate()
    {
        var project = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "重复授权" });
        await _service.GrantPermissionAsync("client-1", project.Id);

        await Assert.ThrowsAsync<DuplicateWaitObjectException>(() =>
            _service.GrantPermissionAsync("client-1", project.Id));
    }

    [Fact]
    public async Task BatchGrantPermissionAsync_ShouldCreateMultiple()
    {
        var p1 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "批量项目A" });
        var p2 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "批量项目B" });
        var p3 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "批量项目C" });

        var results = await _service.BatchGrantPermissionAsync("client-1", new List<Guid> { p1.Id, p2.Id, p3.Id });

        Assert.Equal(3, results.Count);
        Assert.Contains(results, r => r.ProjectName == "批量项目A");
        Assert.Contains(results, r => r.ProjectName == "批量项目B");
        Assert.Contains(results, r => r.ProjectName == "批量项目C");
        Assert.All(results, r => Assert.Equal("client-1", r.ClientId));
    }

    [Fact]
    public async Task BatchGrantPermissionAsync_ShouldSkipDuplicates()
    {
        var p1 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "已授权项目" });
        var p2 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "新项目" });
        await _service.GrantPermissionAsync("client-1", p1.Id);

        // 再次授予 p1（应跳过）+ p2
        var results = await _service.BatchGrantPermissionAsync("client-1", new List<Guid> { p1.Id, p2.Id });

        Assert.Single(results);
        Assert.Equal("新项目", results[0].ProjectName);
    }

    [Fact]
    public async Task ListPermissionsAsync_ShouldReturnAll()
    {
        var p1 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "列表项目A" });
        var p2 = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "列表项目B" });
        await _service.GrantPermissionAsync("client-1", p1.Id);
        await _service.GrantPermissionAsync("client-2", p2.Id);

        // 不传 clientId - 返回全部
        var all = await _service.ListPermissionsAsync(null);
        Assert.Equal(2, all.Count);

        // 按 clientId 过滤
        var forClient1 = await _service.ListPermissionsAsync("client-1");
        Assert.Single(forClient1);
        Assert.Equal("列表项目A", forClient1[0].ProjectName);
    }

    [Fact]
    public async Task RevokePermissionAsync_ShouldRemovePermission()
    {
        var project = await _service.CreateAsync(new CreateProjectRequest { ProjectName = "待撤销项目" });
        var granted = await _service.GrantPermissionAsync("client-1", project.Id);

        var revoked = await _service.RevokePermissionAsync(granted.Id);
        Assert.True(revoked);

        // 确认权限已被撤销
        var list = await _service.ListPermissionsAsync("client-1");
        Assert.DoesNotContain(list, p => p.ProjectId == project.Id);
    }

    [Fact]
    public async Task RevokePermissionAsync_ShouldReturnFalse_WhenNotExists()
    {
        var result = await _service.RevokePermissionAsync(Guid.NewGuid());
        Assert.False(result);
    }

    public void Dispose()
    {
        _fsql.Dispose();
    }
}
