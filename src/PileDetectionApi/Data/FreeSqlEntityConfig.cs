using FreeSql;
using PileDetectionApi.Entities;

namespace PileDetectionApi.Data;

/// <summary>
/// FreeSql Fluent 实体配置
/// 统一管理 C# 属性 → 数据库列名的映射（PascalCase → snake_case）
/// 实体类中无需再写 [Column(Name = "...")]
/// 切换 Oracle 时只需修改此处映射名大小写即可
/// </summary>
public static class FreeSqlEntityConfig
{
    public static void Configure(IFreeSql fsql)
    {
        // ===== ProjectInfoEntity → project_info =====
        fsql.CodeFirst.Entity<ProjectInfoEntity>(eb =>
        {
            eb.Property(p => p.ProjectName).HasColumnName("project_name");
            eb.Property(p => p.ProjectNo).HasColumnName("project_no");
            eb.Property(p => p.ProjectLocation).HasColumnName("project_location");
            eb.Property(p => p.ProjectManager).HasColumnName("project_manager");
            eb.Property(p => p.ProjectDesc).HasColumnName("project_desc");
            eb.Property(p => p.IsDeleted).HasColumnName("is_deleted");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            eb.Property(p => p.ApiVersion).HasColumnName("api_version");
        });

        // ===== PileInfoEntity → pile_info =====
        fsql.CodeFirst.Entity<PileInfoEntity>(eb =>
        {
            eb.Property(p => p.ProjectId).HasColumnName("project_id");
            eb.Property(p => p.PileName).HasColumnName("pile_name");
            eb.Property(p => p.DesignLength).HasColumnName("design_length");
            eb.Property(p => p.DesignDiameter).HasColumnName("design_diameter");
            eb.Property(p => p.DesignStrength).HasColumnName("design_strength");
            eb.Property(p => p.PourDate).HasColumnName("pour_date");
            eb.Property(p => p.TestDate).HasColumnName("test_date");
            eb.Property(p => p.TestStandard).HasColumnName("test_standard");
            eb.Property(p => p.InstrumentModel).HasColumnName("instrument_model");
            eb.Property(p => p.InstrumentSn).HasColumnName("instrument_sn");
            eb.Property(p => p.CertificationNo).HasColumnName("certification_no");
            eb.Property(p => p.Tester).HasColumnName("tester");
            eb.Property(p => p.TesterCertNo).HasColumnName("tester_cert_no");
            eb.Property(p => p.IntegrityCategory).HasColumnName("integrity_category");
            eb.Property(p => p.IsDeleted).HasColumnName("is_deleted");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            eb.Property(p => p.ApiVersion).HasColumnName("api_version");
        });

        // ===== ProfileStatEntity → profile_stat =====
        fsql.CodeFirst.Entity<ProfileStatEntity>(eb =>
        {
            eb.Property(p => p.PileInfoId).HasColumnName("pile_info_id");
            eb.Property(p => p.Distance).HasColumnName("distance");
            eb.Property(p => p.MaxVelocity).HasColumnName("max_velocity");
            eb.Property(p => p.MinVelocity).HasColumnName("min_velocity");
            eb.Property(p => p.AvgVelocity).HasColumnName("avg_velocity");
            eb.Property(p => p.StdVelocity).HasColumnName("std_velocity");
            eb.Property(p => p.CvVelocity).HasColumnName("cv_velocity");
            eb.Property(p => p.CriticalVelocity).HasColumnName("critical_velocity");
            eb.Property(p => p.MaxAmplitude).HasColumnName("max_amplitude");
            eb.Property(p => p.MinAmplitude).HasColumnName("min_amplitude");
            eb.Property(p => p.AvgAmplitude).HasColumnName("avg_amplitude");
            eb.Property(p => p.StdAmplitude).HasColumnName("std_amplitude");
            eb.Property(p => p.CvAmplitude).HasColumnName("cv_amplitude");
            eb.Property(p => p.CriticalAmplitude).HasColumnName("critical_amplitude");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            eb.Property(p => p.ApiVersion).HasColumnName("api_version");
        });

        // ===== MeasurementDataEntity → measurement_data =====
        fsql.CodeFirst.Entity<MeasurementDataEntity>(eb =>
        {
            eb.Property(p => p.PileInfoId).HasColumnName("pile_info_id");
            eb.Property(p => p.SoundVelocity).HasColumnName("sound_velocity");
            eb.Property(p => p.Amplitude).HasColumnName("amplitude");
            eb.Property(p => p.SoundTime).HasColumnName("sound_time");
            eb.Property(p => p.Psd).HasColumnName("psd");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            eb.Property(p => p.ApiVersion).HasColumnName("api_version");
        });

        // ===== PileReportEntity → pile_report =====
        fsql.CodeFirst.Entity<PileReportEntity>(eb =>
        {
            eb.Property(p => p.PileInfoId).HasColumnName("pile_info_id");
            eb.Property(p => p.ReportNo).HasColumnName("report_no");
            eb.Property(p => p.ReportDate).HasColumnName("report_date");
            eb.Property(p => p.IntegrityCategory).HasColumnName("integrity_category");
            eb.Property(p => p.AvgVelocity).HasColumnName("avg_velocity");
            eb.Property(p => p.CriticalVelocity).HasColumnName("critical_velocity");
            eb.Property(p => p.AvgAmplitude).HasColumnName("avg_amplitude");
            eb.Property(p => p.CriticalAmplitude).HasColumnName("critical_amplitude");
            eb.Property(p => p.Conclusion).HasColumnName("conclusion");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            eb.Property(p => p.ApiVersion).HasColumnName("api_version");
        });

        // ===== ProjectReportEntity → project_report =====
        fsql.CodeFirst.Entity<ProjectReportEntity>(eb =>
        {
            eb.Property(p => p.ProjectId).HasColumnName("project_id");
            eb.Property(p => p.ReportNo).HasColumnName("report_no");
            eb.Property(p => p.ReportDate).HasColumnName("report_date");
            eb.Property(p => p.Conclusion).HasColumnName("conclusion");
            eb.Property(p => p.IntegritySummary).HasColumnName("integrity_summary");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            eb.Property(p => p.ApiVersion).HasColumnName("api_version");
        });

        // ===== ApiLogEntity → api_log =====
        fsql.CodeFirst.Entity<ApiLogEntity>(eb =>
        {
            eb.Property(p => p.Endpoint).HasColumnName("endpoint");
            eb.Property(p => p.HttpMethod).HasColumnName("http_method");
            eb.Property(p => p.RequestBody).HasColumnName("request_body");
            eb.Property(p => p.ResponseCode).HasColumnName("response_code");
            eb.Property(p => p.ResponseBody).HasColumnName("response_body");
            eb.Property(p => p.ClientIp).HasColumnName("client_ip");
            eb.Property(p => p.DurationMs).HasColumnName("duration_ms");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        // ===== ApiKeyEntity → api_key =====
        fsql.CodeFirst.Entity<ApiKeyEntity>(eb =>
        {
            eb.Property(p => p.ClientId).HasColumnName("client_id");
            eb.Property(p => p.ClientName).HasColumnName("client_name");
            eb.Property(p => p.ApiKeyHash).HasColumnName("api_key_hash");
            eb.Property(p => p.ExpiresAt).HasColumnName("expires_at");
            eb.Property(p => p.CreatedBy).HasColumnName("created_by");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
            eb.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        });

        // ===== ProjectPermissionEntity → project_permission =====
        fsql.CodeFirst.Entity<ProjectPermissionEntity>(eb =>
        {
            eb.Property(p => p.ClientId).HasColumnName("client_id");
            eb.Property(p => p.ProjectId).HasColumnName("project_id");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        // ===== MeasurementAuditLogEntity → measurement_audit_log =====
        fsql.CodeFirst.Entity<MeasurementAuditLogEntity>(eb =>
        {
            eb.Property(p => p.MeasurementId).HasColumnName("measurement_id");
            eb.Property(p => p.OperationType).HasColumnName("operation_type");
            eb.Property(p => p.ClientId).HasColumnName("client_id");
            eb.Property(p => p.PreviousData).HasColumnName("previous_data");
            eb.Property(p => p.NewData).HasColumnName("new_data");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        // ===== MeasurementRawWaveformEntity → measurement_raw_waveform =====
        fsql.CodeFirst.Entity<MeasurementRawWaveformEntity>(eb =>
        {
            eb.Property(p => p.MeasurementDataId).HasColumnName("measurement_data_id");
            eb.Property(p => p.PileInfoId).HasColumnName("pile_info_id");
            eb.Property(p => p.SamplingRate).HasColumnName("sampling_rate");
            eb.Property(p => p.PointCount).HasColumnName("point_count");
            eb.Property(p => p.StorageType).HasColumnName("storage_type");
            eb.Property(p => p.RawPointsJson).HasColumnName("raw_points_json");
            eb.Property(p => p.FilePath).HasColumnName("file_path");
            eb.Property(p => p.CreatedAt).HasColumnName("created_at");
        });
    }
}
