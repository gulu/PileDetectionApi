using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IPileService
{
    Task<PileResponse> CreateAsync(Guid projectId, CreatePileRequest request);
    Task<PileResponse?> GetByIdAsync(Guid id);
    Task<PileDetailResponse?> GetDetailAsync(Guid id);
    Task<PagedResponse<PileSummaryResponse>> GetByProjectIdAsync(Guid projectId, int page, int pageSize);
    Task<PileResponse> UpdateAsync(Guid id, UpdatePileRequest request);
    Task<bool> DeleteAsync(Guid id);
}
