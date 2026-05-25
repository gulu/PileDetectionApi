namespace PileDetectionApi.Configs;

/// <summary>
/// 数据库配置（支持 SQLite、PostgreSQL、Oracle、MySQL）
/// </summary>
public class DatabaseConfig
{
    /// <summary>数据库类型: Sqlite / PostgreSQL / Oracle / MySQL</summary>
    public string Provider { get; set; } = "Sqlite";
    public string ConnectionString { get; set; } = "Data Source=dbdata/pile.db";
}
