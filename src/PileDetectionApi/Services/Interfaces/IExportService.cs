namespace PileDetectionApi.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportPileReportAsync(Guid pileInfoId);
    Task<byte[]> ExportProjectReportAsync(Guid projectId);
}
