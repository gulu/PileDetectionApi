namespace PileDetectionApi.Configs;

public class LoggingConfig
{
    public FileLogConfig File { get; set; } = new();
    public bool Table { get; set; } = true;
}

public class FileLogConfig
{
    public string Path { get; set; } = "Logs/pile-{Date}.log";
    public int RetentionDays { get; set; } = 30;
}
