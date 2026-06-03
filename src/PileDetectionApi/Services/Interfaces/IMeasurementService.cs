using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;

namespace PileDetectionApi.Services.Interfaces;

public interface IMeasurementService
{
    Task<List<MeasurementResponse>> CreateBatchAsync(Guid pileInfoId, List<CreateMeasurementRequest> requests, string clientId);
    Task<List<MeasurementResponse>> GetByPileIdAsync(Guid pileInfoId, double? minDepth = null, double? maxDepth = null);
    Task<List<MeasurementResponse>> GetByProfileAsync(Guid pileInfoId, string profile);
    Task<MeasurementResponse> UpdateAsync(Guid id, UpdateMeasurementRequest request, string clientId);
    Task<bool> DeleteAsync(Guid id, string clientId);

    /// <summary>获取单条测点的原始波形矩阵数据（可能为 null）</summary>
    Task<MeasurementWaveformResponse?> GetWaveformAsync(Guid measurementDataId);
}
