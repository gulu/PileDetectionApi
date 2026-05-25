using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IProjectService
{
    Task<ProjectResponse> CreateAsync(CreateProjectRequest request);
    Task<ProjectResponse?> GetByIdAsync(Guid id);
    Task<ProjectDetailResponse?> GetDetailAsync(Guid id);
    Task<PagedResponse<ProjectResponse>> GetPagedAsync(int page, int pageSize, string? keyword);
    Task<PagedResponse<ProjectResponse>> GetPermittedPagedAsync(string clientId, int page, int pageSize, string? keyword);
    Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<int> GetPileCountAsync(Guid projectId);
}
