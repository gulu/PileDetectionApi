using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PileDetectionApi.Configs;
using PileDetectionApi.Converters;
using PileDetectionApi.Data;
using PileDetectionApi.Mappings;
using PileDetectionApi.Middleware;
using PileDetectionApi.Services;
using PileDetectionApi.Services.Interfaces;
using Serilog;

// 设置控制台编码为 UTF-8，确保中文日志正常显示
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog 日志配置 =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File(
        path: builder.Configuration.GetValue<string>("Logging:File:Path") ?? "Logs/pile-{Date}.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: builder.Configuration.GetValue<int>("Logging:File:RetentionDays", 30),
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ===== 配置绑定 =====
builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("Database"));
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<LoggingConfig>(builder.Configuration.GetSection("Logging"));
builder.Services.Configure<AdminConfig>(builder.Configuration.GetSection("Admin"));

// ===== FreeSql 注册 =====
var dbConfig = builder.Configuration.GetSection("Database").Get<DatabaseConfig>()
               ?? new DatabaseConfig();

// 解析 dbdata 目录路径：开发时定位到项目 dbdata/，发布后定位到 ContentRoot/dbdata/
if (builder.Environment.IsDevelopment())
{
    // ContentRootPath = src/PileDetectionApi/，直接使用 dbdata/ 子目录
    dbConfig.ConnectionString = $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "dbdata", "pile.db")}";
}
else
{
    var dataDir = Path.Combine(builder.Environment.ContentRootPath, "dbdata");
    Directory.CreateDirectory(dataDir);
    dbConfig.ConnectionString = $"Data Source={Path.Combine(dataDir, "pile.db")}";
}

var fsql = FreeSqlSetup.CreateFreeSql(dbConfig);
builder.Services.AddSingleton(fsql);

// ===== JWT 认证 =====
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtConfig>() ?? new JwtConfig();
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? jwtConfig.Secret;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// ===== 服务注册 =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IPileService, PileService>();
builder.Services.AddScoped<IProfileStatService, ProfileStatService>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IExportService, ExportService>();

// ===== AutoMapper =====
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ===== FluentValidation =====
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// ===== Controllers + Swagger =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateTimeConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = false;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "基桩超声波透射检测数据采集接口平台 API",
        Version = "v1",
        Description = "基桩超声波透射检测数据采集接口平台"
    });

    // Swagger JWT 支持
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== 中间件管道 =====
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAll");


if (app.Configuration.GetValue<bool>("EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 根路径重定向到 Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});


Log.Information("PileDetectionApi 启动完成，数据库类型: {DbType}", dbConfig.Provider);
Log.Information("Swagger UI: http://localhost:5000/swagger");

app.Run();

// 使 Program 类对测试项目可见（WebApplicationFactory 需要）
public partial class Program { }
