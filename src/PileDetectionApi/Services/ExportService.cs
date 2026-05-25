using ClosedXML.Excel;
using FreeSql;
using PileDetectionApi.Entities;
using PileDetectionApi.Services.Interfaces;

namespace PileDetectionApi.Services;

public class ExportService : IExportService
{
    private readonly IFreeSql _fsql;
    private readonly ILogger<ExportService> _logger;

    public ExportService(IFreeSql fsql, ILogger<ExportService> logger)
    {
        _fsql = fsql;
        _logger = logger;
    }

    /// <summary>
    /// 导出单桩报告 xlsx，格式参照 doc/需求/pile1.xlsx
    /// Sheet 1: 桩信息, Sheet 2: 数据表, Sheet 3: 单桩报告
    /// </summary>
    public async Task<byte[]> ExportPileReportAsync(Guid pileInfoId)
    {
        var pile = await _fsql.Select<PileInfoEntity>()
            .Where(p => p.Id == pileInfoId && !p.IsDeleted)
            .Include(p => p.Project)
            .IncludeMany(p => p.ProfileStats)
            .IncludeMany(p => p.Measurements)
            .Include(p => p.Report)
            .FirstAsync() ?? throw new KeyNotFoundException($"基桩不存在: Id={pileInfoId}");

        using var workbook = new XLWorkbook();

        // ========== Sheet 1: 桩信息 ==========
        var ws1 = workbook.Worksheets.Add("桩信息");
        ws1.Cell(1, 1).Value = "工程名称";
        ws1.Cell(1, 2).Value = pile.Project?.ProjectName ?? "";
        ws1.Cell(1, 4).Value = "基桩名称";
        ws1.Cell(1, 5).Value = pile.PileName;

        ws1.Cell(2, 1).Value = "设计桩长";
        ws1.Cell(2, 2).Value = pile.DesignLength.HasValue ? $"{pile.DesignLength:F3}m" : "";
        ws1.Cell(2, 4).Value = "设计桩径";
        ws1.Cell(2, 5).Value = pile.DesignDiameter.HasValue ? $"{pile.DesignDiameter} mm" : "";
        ws1.Cell(2, 7).Value = "设计强度";
        ws1.Cell(2, 8).Value = pile.DesignStrength ?? "";

        ws1.Cell(3, 1).Value = "浇筑日期";
        ws1.Cell(3, 2).Value = pile.PourDate?.ToString("yyyy年 M月d日") ?? "";
        ws1.Cell(3, 4).Value = "测试日期";
        ws1.Cell(3, 5).Value = pile.TestDate?.ToString("yyyy年 M月d日") ?? "";
        ws1.Cell(3, 7).Value = "检测依据";
        ws1.Cell(3, 8).Value = pile.TestStandard ?? "";

        ws1.Cell(4, 1).Value = "仪器型号";
        ws1.Cell(4, 2).Value = pile.InstrumentModel ?? "";
        ws1.Cell(4, 4).Value = "仪器编号";
        ws1.Cell(4, 5).Value = pile.InstrumentSn ?? "";
        ws1.Cell(4, 7).Value = "检定证号";
        ws1.Cell(4, 8).Value = pile.CertificationNo ?? "";

        ws1.Cell(5, 1).Value = "测试人员";
        ws1.Cell(5, 2).Value = pile.Tester ?? "";
        ws1.Cell(5, 4).Value = "上岗证号";
        ws1.Cell(5, 5).Value = pile.TesterCertNo ?? "";

        ws1.Columns().AdjustToContents();

        // ========== Sheet 2: 数据表 ==========
        var ws2 = workbook.Worksheets.Add("数据表");
        var profiles = pile.ProfileStats;

        // 写入表头：剖面行
        int col = 1;
        ws2.Cell(1, col++).Value = "";
        foreach (var ps in profiles)
        {
            ws2.Cell(1, col).Value = $"剖面\t{ps.Profile}";
            ws2.Cell(1, col + 1).Value = "";
            ws2.Cell(1, col + 2).Value = "";
            ws2.Cell(2, col).Value = "测距";
            ws2.Cell(2, col + 1).Value = ps.Distance.HasValue ? $"{ps.Distance}mm" : "";
            col += 3;
        }

        // 统计值行
        col = 1;
        ws2.Cell(3, 1).Value = "";
        foreach (var ps in profiles)
        {
            ws2.Cell(3, col + 1).Value = "声速(km/s)";
            ws2.Cell(3, col + 2).Value = "幅度(dB)";
            col += 3;
        }

        var stats = new (string label, Func<ProfileStatEntity, double?> velocity, Func<ProfileStatEntity, double?> amplitude)[]
        {
            ("最大值", p => p.MaxVelocity, p => p.MaxAmplitude),
            ("最小值", p => p.MinVelocity, p => p.MinAmplitude),
            ("平均值", p => p.AvgVelocity, p => p.AvgAmplitude),
            ("标准差", p => p.StdVelocity, p => p.StdAmplitude),
            ("离差", p => p.CvVelocity, p => p.CvAmplitude),
            ("临界值", p => p.CriticalVelocity, p => p.CriticalAmplitude),
        };

        int row = 4;
        foreach (var (label, vFunc, aFunc) in stats)
        {
            col = 1;
            ws2.Cell(row, col++).Value = label;
            foreach (var ps in profiles)
            {
                ws2.Cell(row, col).Value = FormatNum(vFunc(ps));
                ws2.Cell(row, col + 1).Value = FormatNum(aFunc(ps));
                col += 3;
            }
            row++;
        }

        row++; // 空行
        row++; // 数据表头

        // 数据表列头
        col = 1;
        ws2.Cell(row, col++).Value = "深度(m)";
        foreach (var ps in profiles)
        {
            ws2.Cell(row, col++).Value = "波速(km/s)";
            ws2.Cell(row, col++).Value = "幅度(dB)";
            ws2.Cell(row, col++).Value = "声时(us)";
            ws2.Cell(row, col++).Value = "PSD(us^2/cm)";
        }

        row++;

        // 获取该桩所有测点数据，按深度分组
        var measurements = pile.Measurements
            .GroupBy(m => m.Depth)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var depthGroup in measurements)
        {
            col = 1;
            ws2.Cell(row, col++).Value = depthGroup.Key;

            foreach (var ps in profiles)
            {
                var m = depthGroup.FirstOrDefault(x => x.Profile == ps.Profile);
                if (m != null)
                {
                    ws2.Cell(row, col).Value = FormatNum(m.SoundVelocity);
                    ws2.Cell(row, col + 1).Value = FormatNum(m.Amplitude);
                    ws2.Cell(row, col + 2).Value = FormatNum(m.SoundTime);
                    ws2.Cell(row, col + 3).Value = FormatNum(m.Psd);
                }
                col += 4;
            }
            row++;
        }

        ws2.Columns().AdjustToContents();

        // ========== Sheet 3: 单桩报告 ==========
        var ws3 = workbook.Worksheets.Add("单桩报告");
        ws3.Cell(1, 1).Value = "超声波法单桩检测报告单";
        ws3.Cell(1, 1).Style.Font.Bold = true;
        ws3.Cell(1, 1).Style.Font.FontSize = 14;

        ws3.Cell(3, 1).Value = "项目名称";
        ws3.Cell(3, 2).Value = pile.Project?.ProjectName ?? "";
        ws3.Cell(3, 5).Value = "桩编号";
        ws3.Cell(3, 6).Value = pile.PileName;

        ws3.Cell(4, 1).Value = "检测依据";
        ws3.Cell(4, 2).Value = pile.TestStandard ?? "";

        ws3.Cell(5, 1).Value = "测试仪器";
        ws3.Cell(5, 2).Value = pile.InstrumentModel ?? "";

        ws3.Cell(6, 1).Value = "灌注日期";
        ws3.Cell(6, 2).Value = pile.PourDate?.ToString("yyyy年 M月d日") ?? "";
        ws3.Cell(6, 4).Value = "测试日期";
        ws3.Cell(6, 5).Value = pile.TestDate?.ToString("yyyy年 M月d日") ?? "";
        ws3.Cell(6, 7).Value = "设计砼强度";
        ws3.Cell(6, 8).Value = pile.DesignStrength ?? "";

        ws3.Cell(7, 1).Value = "桩型";
        ws3.Cell(7, 2).Value = "摩擦桩";
        ws3.Cell(7, 4).Value = "设计桩长(m)";
        ws3.Cell(7, 5).Value = pile.DesignLength.HasValue ? $"{pile.DesignLength:F3}" : "";
        ws3.Cell(7, 7).Value = "设计桩径(m)";
        ws3.Cell(7, 8).Value = pile.DesignDiameter.HasValue ? $"{pile.DesignDiameter / 1000.0:F2}" : "";

        // 测试结果表格
        ws3.Cell(9, 1).Value = "组号";
        ws3.Cell(9, 2).Value = "Vm(km/s)";
        ws3.Cell(9, 3).Value = "Am(dB)";
        ws3.Cell(9, 4).Value = "VD(km/s)";
        ws3.Cell(9, 5).Value = "AD(dB)";

        int r = 10;
        foreach (var ps in profiles)
        {
            ws3.Cell(r, 1).Value = ps.Profile;
            ws3.Cell(r, 2).Value = FormatNum(ps.AvgVelocity);
            ws3.Cell(r, 3).Value = FormatNum(ps.AvgAmplitude);
            ws3.Cell(r, 4).Value = FormatNum(ps.CriticalVelocity);
            ws3.Cell(r, 5).Value = FormatNum(ps.CriticalAmplitude);
            r++;
        }

        r += 2;
        ws3.Cell(r, 1).Value = "检测结果";
        ws3.Cell(r, 1).Style.Font.Bold = true;
        r++;

        if (pile.Report != null)
        {
            ws3.Cell(r, 1).Value = $"整桩声速临界值：{FormatNum(pile.Report.CriticalVelocity)} km/s。";
            r++;
            ws3.Cell(r, 1).Value = pile.Report.Conclusion ?? "";
            r++;
            ws3.Cell(r, 1).Value = $"完整性类别：{pile.Report.IntegrityCategory}类桩";
        }

        ws3.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        _logger.LogInformation("单桩 xlsx 导出成功: PileId={PileId}", pileInfoId);

        return stream.ToArray();
    }

    /// <summary>
    /// 导出项目汇总 xlsx
    /// Sheet 1: 项目概况, Sheet 2: 各桩统计, Sheet 3: 逐桩报告
    /// </summary>
    public async Task<byte[]> ExportProjectReportAsync(Guid projectId)
    {
        var project = await _fsql.Select<ProjectInfoEntity>()
            .Where(p => p.Id == projectId && !p.IsDeleted)
            .IncludeMany(p => p.Piles)
            .FirstAsync() ?? throw new KeyNotFoundException($"项目不存在: Id={projectId}");

        using var workbook = new XLWorkbook();

        // Sheet 1: 项目概况
        var ws1 = workbook.Worksheets.Add("项目概况");
        ws1.Cell(1, 1).Value = "项目名称";
        ws1.Cell(1, 2).Value = project.ProjectName;
        ws1.Cell(2, 1).Value = "项目编号";
        ws1.Cell(2, 2).Value = project.ProjectNo ?? "";
        ws1.Cell(3, 1).Value = "项目地点";
        ws1.Cell(3, 2).Value = project.ProjectLocation ?? "";
        ws1.Cell(4, 1).Value = "项目负责人";
        ws1.Cell(4, 2).Value = project.ProjectManager ?? "";
        ws1.Cell(5, 1).Value = "基桩数量";
        ws1.Cell(5, 2).Value = project.Piles.Count;

        // Sheet 2: 各桩统计
        var ws2 = workbook.Worksheets.Add("各桩统计");
        ws2.Cell(1, 1).Value = "桩名";
        ws2.Cell(1, 2).Value = "设计桩长(m)";
        ws2.Cell(1, 3).Value = "设计桩径(mm)";
        ws2.Cell(1, 4).Value = "设计强度";
        ws2.Cell(1, 5).Value = "测试日期";
        ws2.Cell(1, 6).Value = "完整性类别";

        int row = 2;
        foreach (var pile in project.Piles.Where(p => !p.IsDeleted))
        {
            ws2.Cell(row, 1).Value = pile.PileName;
            ws2.Cell(row, 2).Value = FormatNum(pile.DesignLength);
            ws2.Cell(row, 3).Value = pile.DesignDiameter;
            ws2.Cell(row, 4).Value = pile.DesignStrength ?? "";
            ws2.Cell(row, 5).Value = pile.TestDate?.ToString("yyyy-MM-dd") ?? "";
            ws2.Cell(row, 6).Value = pile.IntegrityCategory.HasValue ? $"{pile.IntegrityCategory}类" : "";
            row++;
        }

        ws2.Columns().AdjustToContents();

        // Sheet 3: 逐桩报告（每桩一页）
        var ws3 = workbook.Worksheets.Add("逐桩报告");
        row = 1;
        foreach (var pile in project.Piles.Where(p => !p.IsDeleted))
        {
            ws3.Cell(row, 1).Value = $"基桩: {pile.PileName}";
            ws3.Cell(row, 1).Style.Font.Bold = true;
            row++;
            ws3.Cell(row, 1).Value = $"设计桩长: {FormatNum(pile.DesignLength)}m";
            ws3.Cell(row, 2).Value = $"桩径: {pile.DesignDiameter}mm";
            ws3.Cell(row, 3).Value = $"强度: {pile.DesignStrength}";
            row++;
            ws3.Cell(row, 1).Value = $"测试日期: {pile.TestDate?.ToString("yyyy-MM-dd")}";
            ws3.Cell(row, 2).Value = $"完整性: {(pile.IntegrityCategory.HasValue ? $"{pile.IntegrityCategory}类" : "未判定")}";
            row += 2;
        }

        ws3.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        _logger.LogInformation("项目 xlsx 导出成功: ProjectId={ProjectId}", projectId);

        return stream.ToArray();
    }

    private static double? FormatNum(double? value)
        => value.HasValue ? Math.Round(value.Value, 4) : null;
}
