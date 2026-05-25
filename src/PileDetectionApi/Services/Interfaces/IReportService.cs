using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IReportService
{
    Task<PileReportResponse> CreateOrUpdatePileReportAsync(Guid pileInfoId, CreatePileReportRequest request);
    Task<PileReportResponse?> GetPileReportAsync(Guid pileInfoId);

    Task<ProjectReportResponse> CreateOrUpdateProjectReportAsync(Guid projectId, CreateProjectReportRequest request);
    Task<ProjectReportResponse?> GetProjectReportAsync(Guid projectId);
}
