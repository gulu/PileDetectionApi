using AutoMapper;
using PileDetectionApi.DTOs.Request;
using PileDetectionApi.DTOs.Response;
using PileDetectionApi.Entities;

namespace PileDetectionApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Project
        CreateMap<CreateProjectRequest, ProjectInfoEntity>();
        CreateMap<UpdateProjectRequest, ProjectInfoEntity>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<ProjectInfoEntity, ProjectResponse>();
        CreateMap<ProjectInfoEntity, ProjectDetailResponse>();

        // Pile
        CreateMap<CreatePileRequest, PileInfoEntity>();
        CreateMap<UpdatePileRequest, PileInfoEntity>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<PileInfoEntity, PileResponse>();
        CreateMap<PileInfoEntity, PileDetailResponse>();
        CreateMap<PileInfoEntity, PileSummaryResponse>();

        // ProfileStat
        CreateMap<CreateProfileStatRequest, ProfileStatEntity>();
        CreateMap<UpdateProfileStatRequest, ProfileStatEntity>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<ProfileStatEntity, ProfileStatResponse>();

        // Measurement
        CreateMap<CreateMeasurementRequest, MeasurementDataEntity>()
            .ForMember(d => d.Id, opts => opts.Ignore())
            .ForMember(d => d.PileInfoId, opts => opts.Ignore())
            .ForMember(d => d.CreatedAt, opts => opts.Ignore())
            .ForMember(d => d.UpdatedAt, opts => opts.Ignore())
            .ForMember(d => d.ApiVersion, opts => opts.Ignore());
        CreateMap<UpdateMeasurementRequest, MeasurementDataEntity>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<MeasurementDataEntity, MeasurementResponse>()
            .ForMember(d => d.HasWaveform, opts => opts.Ignore());

        // MeasurementRawWaveform
        CreateMap<MeasurementWaveformRequest, MeasurementRawWaveformEntity>();
        CreateMap<MeasurementRawWaveformEntity, MeasurementWaveformResponse>();

        // PileReport
        CreateMap<CreatePileReportRequest, PileReportEntity>();
        CreateMap<UpdatePileReportRequest, PileReportEntity>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<PileReportEntity, PileReportResponse>();

        // ProjectReport
        CreateMap<CreateProjectReportRequest, ProjectReportEntity>();
        CreateMap<UpdateProjectReportRequest, ProjectReportEntity>()
            .ForAllMembers(opts => opts.Condition((_, _, srcMember) => srcMember != null));
        CreateMap<ProjectReportEntity, ProjectReportResponse>();
    }
}
