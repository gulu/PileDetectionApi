using FreeSql;
using PileDetectionApi.Configs;
using PileDetectionApi.Entities;

namespace PileDetectionApi.Data;

/// <summary>
/// FreeSql 初始化配置，支持多数据库切换及命名规则自动转换
/// </summary>
public static class FreeSqlSetup
{
    public static IFreeSql CreateFreeSql(DatabaseConfig config)
    {
        var dbType = config.Provider.ToLower() switch
        {
            "sqlite" => DataType.Sqlite,
            "postgresql" => DataType.PostgreSQL,
            "oracle" => DataType.Oracle,
            "mysql" => DataType.MySql,
            _ => DataType.Sqlite
        };

        var fsql = new FreeSqlBuilder()
            .UseConnectionString(dbType, config.ConnectionString)
            .UseAutoSyncStructure(true)
            .Build();

        // 注册 Fluent 实体列名映射（PascalCase → snake_case）
        FreeSqlEntityConfig.Configure(fsql);

        // 同步实体结构到数据库（CodeFirst）
        fsql.CodeFirst.SyncStructure(
            typeof(ProjectInfoEntity),
            typeof(PileInfoEntity),
            typeof(ProfileStatEntity),
            typeof(MeasurementDataEntity),
            typeof(PileReportEntity),
            typeof(ProjectReportEntity),
            typeof(ApiLogEntity),
            typeof(ApiKeyEntity),
            typeof(ProjectPermissionEntity),
            typeof(MeasurementAuditLogEntity),
            typeof(MeasurementRawWaveformEntity)
        );

        // 首次启动时创建默认 API Key（仅在 api_key 表为空时）
        if (!fsql.Select<ApiKeyEntity>().Any())
        {
            var defaultClientId = "default-client";
            var defaultKeyPlain = "pile-detection-secret-key-2026";
            var defaultHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(defaultKeyPlain)))
                .ToLowerInvariant();

            fsql.Insert(new ApiKeyEntity
            {
                ClientId = defaultClientId,
                ClientName = "默认客户端",
                ApiKeyHash = defaultHash,
                Status = 1,
                CreatedBy = "system"
            }).ExecuteAffrows();
        }

        return fsql;
    }
}
