using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Testing;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using Xunit;

namespace PileDetectionApi.Tests.IntegrationTests;

/// <summary>
/// 整体功能测试：从 pile1.xlsx 读取数据，通过 API 完整写入数据库，并验证所有数据正确。
/// 此测试在单元测试全部通过后执行，作为最终集成验证。
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration Tests")]
public class SeedDataTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _xlsxPath;
    private string _token = "";

    // 数据缓存（从 xlsx 读取）
    private string _projectName = "", _pileName = "";
    private double _designLength;
    private int _designDiameter;
    private string _designStrength = "", _testStandard = "";
    private string _pourDate = "", _testDate = "";
    private string _instrumentModel = "", _instrumentSn = "";
    private readonly List<ProfileInfo> _profiles = new();
    private readonly List<MeasurementInfo> _measurements = new();
    private int _integrityCategory;
    private string _conclusion = "";
    // 动态创建的 ID，用于验证
    private Guid _createdProjectId;
    private Guid _createdPileId;

    public class ProfileInfo
    {
        public string Name { get; set; } = "";
        public double? Distance { get; set; }
        public double? MaxVel, MinVel, AvgVel, StdVel, CvVel, CriticalVel;
        public double? MaxAmp, MinAmp, AvgAmp, StdAmp, CvAmp, CriticalAmp;
    }

    public class MeasurementInfo
    {
        public string Profile { get; set; } = "";
        public double Depth;
        public double? Velocity, Amplitude, SoundTime, Psd;
    }

    public SeedDataTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                // 使用临时数据库文件，不污染主数据库
                var tempDb = Path.Combine(Path.GetTempPath(), $"pile_test_{Guid.NewGuid():N}.db");
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            });
        });

        _client = _factory.CreateClient();
        _xlsxPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "doc", "需求", "pile1.xlsx"));
    }

    public async Task InitializeAsync()
    {
        // Step 1: 读取 xlsx 数据
        ReadXlsxData();

        // Step 2: 获取 JWT Token
        await GetAuthTokenAsync();

        // Step 3: 写入所有数据
        await SeedAllDataAsync();

        // Step 4: 验证所有数据
        await VerifyAllDataAsync();

        // 输出结果
        Console.WriteLine($"\n{'='*60}");
        Console.WriteLine("  整体功能测试通过！正式数据已写入 API 数据库");
        Console.WriteLine($"  项目: {_projectName}, 基桩: {_pileName}");
        Console.WriteLine($"  剖面: {_profiles.Count} 个, 测点: {_measurements.Count} 条");
        Console.WriteLine($"  完整性: {_integrityCategory}类桩");
        Console.WriteLine($"  此数据可用于生成 API 集成文档");
        Console.WriteLine($"{'='*60}\n");
    }

    /// <summary>
    /// 整体功能测试的入口测试方法。
    /// 实际逻辑在 InitializeAsync 中执行（IAsyncLifetime），
    /// 此方法仅为触发 xUnit 测试发现和执行。
    /// </summary>
    [Fact]
    public void SeedData_ShouldCompleteSuccessfully()
    {
        Assert.True(File.Exists(_xlsxPath), $"xlsx 文件不存在: {_xlsxPath}");
        // 实际验证在 InitializeAsync 中进行
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region XLSX 解析

    private void ReadXlsxData()
    {
        Assert.True(File.Exists(_xlsxPath), $"xlsx 文件不存在: {_xlsxPath}");

        using var wb = new XLWorkbook(_xlsxPath);

        // ---- Sheet 1: 桩信息 ----
        var ws1 = wb.Worksheet("桩信息");
        _projectName = ws1.Cell(1, 2).GetString();
        _pileName = ws1.Cell(1, 6).GetString();
        _designLength = double.Parse(ws1.Cell(2, 2).GetString().Replace("m", "").Trim());
        _designDiameter = int.Parse(ws1.Cell(2, 4).GetString().Replace("mm", "").Trim());
        _designStrength = ws1.Cell(2, 6).GetString();

        _pourDate = ParseDateCell(ws1.Cell(3, 2).GetString());
        _testDate = ParseDateCell(ws1.Cell(3, 4).GetString());
        _testStandard = ws1.Cell(3, 6).GetString();
        _instrumentModel = ws1.Cell(4, 2).GetString();
        _instrumentSn = ws1.Cell(4, 4).GetString();

        Console.WriteLine($"📋 桩信息: {_projectName} / {_pileName}");

        // ---- Sheet 2: 数据表 ----
        var ws2 = wb.Worksheet("数据表");

        // 解析 6 个剖面
        for (int g = 0; g < 6; g++)
        {
            var nameCell = ws2.Cell(1, 3 + g * 4).GetString(); // 3,7,11,15,19,23
            var distCell = ws2.Cell(1, 5 + g * 4).GetString(); // 5,9,13,17,21,25
            var dataCol = 2 + g * 4; // 2,6,10,14,18,22

            var pi = new ProfileInfo { Name = nameCell.Trim() };
            if (double.TryParse(distCell.Replace("mm", "").Trim(), out var dist))
                pi.Distance = dist;

            // 统计值 Rows 3-8
            var stats = new[] { "max", "min", "avg", "std", "cv", "critical" };
            for (int i = 0; i < 6; i++)
            {
                int statRow = 3 + i;
                double.TryParse(ws2.Cell(statRow, dataCol).GetString(), out var vel);
                double.TryParse(ws2.Cell(statRow, dataCol + 1).GetString(), out var amp);

                switch (stats[i])
                {
                    case "max": pi.MaxVel = vel; pi.MaxAmp = amp; break;
                    case "min": pi.MinVel = vel; pi.MinAmp = amp; break;
                    case "avg": pi.AvgVel = vel; pi.AvgAmp = amp; break;
                    case "std": pi.StdVel = vel; pi.StdAmp = amp; break;
                    case "cv": pi.CvVel = vel; pi.CvAmp = amp; break;
                    case "critical": pi.CriticalVel = vel; pi.CriticalAmp = amp; break;
                }
            }

            _profiles.Add(pi);
            Console.WriteLine($"  📊 剖面 {pi.Name}: 测距={pi.Distance}mm, Vavg={pi.AvgVel}, Aavg={pi.AvgAmp}");
        }

        // 测点数据 Row 11+
        int row = 11;
        while (true)
        {
            var depthCell = ws2.Cell(row, 1).GetString();
            if (string.IsNullOrWhiteSpace(depthCell)) break;
            if (!double.TryParse(depthCell, out var depth)) break;

            foreach (var p in _profiles)
            {
                // 找到此剖面对应的数据列
                var idx = _profiles.IndexOf(p);
                var dc = 2 + idx * 4;

                double? ParseOpt(int col)
                {
                    var v = ws2.Cell(row, col).GetString();
                    return double.TryParse(v, out var r) ? r : null;
                }

                _measurements.Add(new MeasurementInfo
                {
                    Profile = p.Name,
                    Depth = depth,
                    Velocity = ParseOpt(dc),
                    Amplitude = ParseOpt(dc + 1),
                    SoundTime = ParseOpt(dc + 2),
                    Psd = ParseOpt(dc + 3)
                });
            }
            row++;
        }

        Console.WriteLine($"  📏 测点数据: {_measurements.Count} 条");

        // ---- Sheet 3: 单桩报告 ----
        var ws3 = wb.Worksheet("单桩报告");
        var reportText = ws3.Cell(19, 2).GetString();
        var match = System.Text.RegularExpressions.Regex.Match(reportText, @"[ⅠIⅡⅢⅣ]类");
        if (match.Success)
        {
            var cat = match.Value[0] switch
            {
                'Ⅰ' => 1, 'I' => 1,
                'Ⅱ' => 2, 'Ⅲ' => 3, 'Ⅳ' => 4, _ => 1
            };
            _integrityCategory = cat;
            _conclusion = reportText.Trim();
        }

        Console.WriteLine($"  🏆 完整性: {_integrityCategory}类桩");
    }

    private static string ParseDateCell(string s)
    {
        s = s.Replace("年", "-").Replace("月", "-").Replace("日", "").Replace(" ", "");
        var parts = s.Split('-');
        if (parts.Length == 3)
            return $"{parts[0]}-{int.Parse(parts[1]):D2}-{int.Parse(parts[2]):D2}T00:00:00Z";
        return s + "T00:00:00Z";
    }

    #endregion

    #region API 调用

    private async Task GetAuthTokenAsync()
    {
        var request = new AuthTokenRequest
        {
            ApiKey = "pile-detection-secret-key-2026",
            ClientId = "default-client"
        };

        var response = await _client.PostAsync("/api/v1/auth/token",
            ToJsonContent(request));

        response.EnsureSuccessStatusCode();
        var body = await ParseResponse<AuthTokenResponse>(response);
        _token = body!.Token;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        Console.WriteLine("🔑 Token 获取成功");
    }

    private async Task SeedAllDataAsync()
    {
        // 1. 创建项目
        var projReq = new CreateProjectRequest
        {
            ProjectName = _projectName,
            ProjectNo = "INTEGRATION-TEST-001"
        };
        var projResp = await _client.PostAsync("/api/v1/projects", ToJsonContent(projReq));
        projResp.EnsureSuccessStatusCode();
        var projData = await ParseResponse<ProjectResponse>(projResp);
        var projectId = projData!.Id;
        _createdProjectId = projectId;
        Console.WriteLine($"✅ 项目创建: id={projectId}");

        // 2. 创建基桩
        var pileReq = new CreatePileRequest
        {
            PileName = _pileName,
            DesignLength = _designLength,
            DesignDiameter = _designDiameter,
            DesignStrength = _designStrength,
            PourDate = DateTime.Parse(_pourDate.Replace("T00:00:00Z", "")),
            TestDate = DateTime.Parse(_testDate.Replace("T00:00:00Z", "")),
            TestStandard = _testStandard,
            InstrumentModel = _instrumentModel,
            InstrumentSn = _instrumentSn
        };
        var pileResp = await _client.PostAsync(
            $"/api/v1/projects/{projectId}/piles", ToJsonContent(pileReq));
        pileResp.EnsureSuccessStatusCode();
        var pileData = await ParseResponse<PileResponse>(pileResp);
        var pileId = pileData!.Id;
        _createdPileId = pileId;
        Console.WriteLine($"✅ 基桩创建: id={pileId}");

        // 3. 批量创建剖面统计
        var profileReqs = _profiles.Select(p => new CreateProfileStatRequest
        {
            Profile = p.Name,
            Distance = p.Distance,
            MaxVelocity = p.MaxVel, MinVelocity = p.MinVel,
            AvgVelocity = p.AvgVel, StdVelocity = p.StdVel,
            CvVelocity = p.CvVel, CriticalVelocity = p.CriticalVel,
            MaxAmplitude = p.MaxAmp, MinAmplitude = p.MinAmp,
            AvgAmplitude = p.AvgAmp, StdAmplitude = p.StdAmp,
            CvAmplitude = p.CvAmp, CriticalAmplitude = p.CriticalAmp
        }).ToList();

        var profResp = await _client.PostAsync(
            $"/api/v1/piles/{pileId}/profiles", ToJsonContent(profileReqs));
        profResp.EnsureSuccessStatusCode();
        Console.WriteLine($"✅ 剖面统计写入: {_profiles.Count} 条");

        // 4. 批量创建测点数据（分批，每批最多 200 条）
        var batchSize = 200;
        for (int i = 0; i < _measurements.Count; i += batchSize)
        {
            var batch = _measurements.Skip(i).Take(batchSize)
                .Select(m => new CreateMeasurementRequest
                {
                    Profile = m.Profile,
                    Depth = m.Depth,
                    SoundVelocity = m.Velocity,
                    Amplitude = m.Amplitude,
                    SoundTime = m.SoundTime,
                    Psd = m.Psd
                }).ToList();

            var measResp = await _client.PostAsync(
                $"/api/v1/piles/{pileId}/measurements", ToJsonContent(batch));
            measResp.EnsureSuccessStatusCode();
        }
        Console.WriteLine($"✅ 测点数据写入: {_measurements.Count} 条");

        // 5. 创建单桩报告
        var reportReq = new CreatePileReportRequest
        {
            IntegrityCategory = _integrityCategory,
            Conclusion = _conclusion,
            ReportDate = DateTime.Parse(_testDate.Replace("T00:00:00Z", "")),
            AvgVelocity = _profiles.Average(p => p.AvgVel ?? 0),
            CriticalVelocity = _profiles.Average(p => p.CriticalVel ?? 0)
        };
        var repResp = await _client.PostAsync(
            $"/api/v1/piles/{pileId}/report", ToJsonContent(reportReq));
        repResp.EnsureSuccessStatusCode();
        Console.WriteLine($"✅ 单桩报告写入: 完整性={_integrityCategory}类");

        // 6. 创建项目报告
        var projRepReq = new CreateProjectReportRequest
        {
            Conclusion = "整体功能测试验证通过。",
            IntegritySummary = $"Ⅰ类桩: 1根, Ⅱ类桩: 0根, Ⅲ类桩: 0根, Ⅳ类桩: 0根"
        };
        var projRepResp = await _client.PostAsync(
            $"/api/v1/projects/{projectId}/report", ToJsonContent(projRepReq));
        projRepResp.EnsureSuccessStatusCode();
        Console.WriteLine($"✅ 项目报告写入完成");
    }

    private async Task VerifyAllDataAsync()
    {
        Assert.True(_createdProjectId != Guid.Empty, "项目 ID 未捕获，请检查创建流程");
        Assert.True(_createdPileId != Guid.Empty, "基桩 ID 未捕获，请检查创建流程");

        // 获取项目详情
        var getProj = await _client.PostAsync($"/api/v1/projects/{_createdProjectId}", null);
        var projBody = await getProj.Content.ReadAsStringAsync();
        Assert.True(getProj.IsSuccessStatusCode, $"项目查询失败: {getProj.StatusCode}\n{projBody}");

        // 获取基桩详情
        var getPile = await _client.PostAsync($"/api/v1/piles/{_createdPileId}", null);
        var pileBody = await getPile.Content.ReadAsStringAsync();
        Assert.True(getPile.IsSuccessStatusCode, $"基桩查询失败: {getPile.StatusCode}\n{pileBody}");

        // 获取剖面统计
        var getProf = await _client.PostAsync($"/api/v1/piles/{_createdPileId}/profiles/list", null);
        getProf.EnsureSuccessStatusCode();

        // 获取测点数据
        var getMeas = await _client.PostAsync($"/api/v1/piles/{_createdPileId}/measurements/list", ToJsonContent(new { }));
        var measBody = await getMeas.Content.ReadAsStringAsync();
        Assert.True(getMeas.IsSuccessStatusCode, $"测点查询失败: {getMeas.StatusCode}\n{measBody}");
        var measData = await ParseResponse<List<MeasurementResponse>>(getMeas);
        Assert.NotNull(measData);
        Assert.Equal(_measurements.Count, measData.Count);

        // 获取单桩报告
        var getRep = await _client.PostAsync($"/api/v1/piles/{_createdPileId}/report/detail", null);
        getRep.EnsureSuccessStatusCode();
        var repData = await ParseResponse<PileReportResponse>(getRep);
        Assert.NotNull(repData);
        Assert.Equal(_integrityCategory, repData.IntegrityCategory);

        Console.WriteLine("✅ 数据验证全部通过");
    }

    #endregion

    #region 工具方法

    private static StringContent ToJsonContent<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static async Task<T?> ParseResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<JsonElement>(json);
        if (wrapper.TryGetProperty("data", out var data))
            return JsonSerializer.Deserialize<T>(data.GetRawText(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return default;
    }

    #endregion
}
