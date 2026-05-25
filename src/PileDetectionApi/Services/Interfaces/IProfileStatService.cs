using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IProfileStatService
{
    Task<List<ProfileStatResponse>> CreateBatchAsync(Guid pileInfoId, List<CreateProfileStatRequest> requests);
    Task<List<ProfileStatResponse>> GetByPileIdAsync(Guid pileInfoId);
    Task<ProfileStatResponse> UpdateAsync(Guid id, UpdateProfileStatRequest request);
    Task<bool> DeleteAsync(Guid id);
}
