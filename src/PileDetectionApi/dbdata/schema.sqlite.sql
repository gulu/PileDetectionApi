-- ============================================================
-- 超声基桩检测数据管理系统 - 建表脚本（完整版）
-- 数据库: SQLite 兼容
-- 每列均附中文说明（SQL 注释）
-- ============================================================

-- ==================== 项目信息表 ====================
CREATE TABLE IF NOT EXISTS "project_info" (
    "id"               TEXT PRIMARY KEY,                       -- 主键 ID（UUID）
    "project_name"     TEXT NOT NULL,                          -- 项目名称
    "project_no"       TEXT,                                   -- 项目编号
    "project_location" TEXT,                                   -- 项目地点
    "project_manager"  TEXT,                                   -- 项目负责人
    "project_desc"     TEXT,                                   -- 项目描述
    "is_deleted"       INTEGER NOT NULL DEFAULT 0,             -- 软删除标记（0=正常, 1=已删除）
    "created_at"       TEXT NOT NULL DEFAULT (datetime('now')),-- 创建时间
    "updated_at"       TEXT,                                   -- 更新时间
    "api_version"      TEXT NOT NULL DEFAULT 'v1'              -- API 版本
);

-- ==================== 基桩信息表 ====================
CREATE TABLE IF NOT EXISTS "pile_info" (
    "id"               TEXT PRIMARY KEY,                       -- 主键 ID（UUID）
    "project_id"       TEXT NOT NULL,                   -- 所属项目 ID（外键）
    "pile_name"        TEXT NOT NULL,                      -- 桩号（如右11）
    "design_length"    REAL,                               -- 设计桩长（m）
    "design_diameter"  INTEGER,                            -- 设计桩径（mm）
    "design_strength"  TEXT,                               -- 设计强度等级（如C25）
    "pour_date"        TEXT,                               -- 浇筑日期（ISO 8601）
    "test_date"        TEXT,                               -- 检测日期（ISO 8601）
    "test_standard"    TEXT,                               -- 检测标准（如JGJ 106-2014）
    "instrument_model" TEXT,                               -- 仪器型号（如RSM-SY7）
    "instrument_sn"    TEXT,                               -- 仪器编号
    "certification_no" TEXT,                               -- 设备检定证书号
    "tester"           TEXT,                               -- 检测人
    "tester_cert_no"   TEXT,                               -- 检测人证书号
    "integrity_category" INTEGER,                          -- 完整性类别（1=Ⅰ类, 2=Ⅱ类, 3=Ⅲ类, 4=Ⅳ类）
    "is_deleted"       INTEGER NOT NULL DEFAULT 0,         -- 软删除标记
    "created_at"       TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间
    "updated_at"       TEXT,                               -- 更新时间
    "api_version"      TEXT NOT NULL DEFAULT 'v1',         -- API 版本
    FOREIGN KEY ("project_id") REFERENCES "project_info"("id") ON DELETE CASCADE
);

-- ==================== 剖面统计表 ====================
CREATE TABLE IF NOT EXISTS "profile_stat" (
    "id"                INTEGER PRIMARY KEY AUTOINCREMENT,  -- 主键 ID
    "pile_info_id"      TEXT NOT NULL,                   -- 所属基桩 ID（外键）
    "profile"           TEXT NOT NULL,                      -- 剖面名称（如1-2）
    "distance"          REAL,                               -- 测距（mm）
    "max_velocity"      REAL,                               -- 最大声速（km/s）
    "min_velocity"      REAL,                               -- 最小声速（km/s）
    "avg_velocity"      REAL,                               -- 平均声速（km/s）
    "std_velocity"      REAL,                               -- 声速标准差
    "cv_velocity"       REAL,                               -- 声速变异系数（%）
    "critical_velocity" REAL,                               -- 声速临界值（km/s）
    "max_amplitude"     REAL,                               -- 最大波幅（dB）
    "min_amplitude"     REAL,                               -- 最小波幅（dB）
    "avg_amplitude"     REAL,                               -- 平均波幅（dB）
    "std_amplitude"     REAL,                               -- 波幅标准差
    "cv_amplitude"      REAL,                               -- 波幅变异系数（%）
    "critical_amplitude" REAL,                              -- 波幅临界值（dB）
    "created_at"        TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间
    "updated_at"        TEXT,                               -- 更新时间
    "api_version"       TEXT NOT NULL DEFAULT 'v1',         -- API 版本
    FOREIGN KEY ("pile_info_id") REFERENCES "pile_info"("id") ON DELETE CASCADE
);

-- ==================== 测点数据表 ====================
CREATE TABLE IF NOT EXISTS "measurement_data" (
    "id"             INTEGER PRIMARY KEY AUTOINCREMENT,  -- 主键 ID
    "pile_info_id"   INTEGER NOT NULL,                   -- 所属基桩 ID（外键）
    "profile"        TEXT NOT NULL,                      -- 所属剖面名称（如1-2）
    "depth"          REAL NOT NULL,                      -- 深度（m）
    "sound_velocity" REAL,                               -- 声速（km/s）
    "amplitude"      REAL,                               -- 波幅（dB）
    "sound_time"     REAL,                               -- 声时（us）
    "psd"            REAL,                               -- PSD 值
    "created_at"     TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间
    "updated_at"     TEXT,                               -- 更新时间
    "api_version"    TEXT NOT NULL DEFAULT 'v1',         -- API 版本
    FOREIGN KEY ("pile_info_id") REFERENCES "pile_info"("id") ON DELETE CASCADE
);

-- ==================== 单桩报告表 ====================
CREATE TABLE IF NOT EXISTS "pile_report" (
    "id"                INTEGER PRIMARY KEY AUTOINCREMENT,  -- 主键 ID
    "pile_info_id"      TEXT NOT NULL,                   -- 所属基桩 ID（外键，一对一）
    "report_no"         TEXT,                               -- 报告编号
    "report_date"       TEXT,                               -- 报告日期
    "integrity_category" INTEGER,                           -- 完整性类别（1=Ⅰ类, 2=Ⅱ类, 3=Ⅲ类, 4=Ⅳ类）
    "avg_velocity"      REAL,                               -- 平均声速（km/s）
    "critical_velocity" REAL,                               -- 临界声速（km/s）
    "avg_amplitude"     REAL,                               -- 平均波幅（dB）
    "critical_amplitude" REAL,                              -- 临界波幅（dB）
    "conclusion"        TEXT,                               -- 检测结论
    "created_at"        TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间
    "updated_at"        TEXT,                               -- 更新时间
    "api_version"       TEXT NOT NULL DEFAULT 'v1',         -- API 版本
    FOREIGN KEY ("pile_info_id") REFERENCES "pile_info"("id") ON DELETE CASCADE
);

-- ==================== 项目报告表 ====================
CREATE TABLE IF NOT EXISTS "project_report" (
    "id"               TEXT PRIMARY KEY,                       -- 主键 ID（UUID）
    "project_id"       TEXT NOT NULL,                   -- 所属项目 ID（外键）
    "report_no"        TEXT,                               -- 报告编号
    "report_date"      TEXT,                               -- 报告日期
    "conclusion"       TEXT,                               -- 检测结论
    "integrity_summary" TEXT,                              -- 完整性汇总（如Ⅰ类桩: 1根）
    "created_at"       TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间
    "updated_at"       TEXT,                               -- 更新时间
    "api_version"      TEXT NOT NULL DEFAULT 'v1',         -- API 版本
    FOREIGN KEY ("project_id") REFERENCES "project_info"("id") ON DELETE CASCADE
);

-- ==================== API 日志表 ====================
CREATE TABLE IF NOT EXISTS "api_log" (
    "id"            INTEGER PRIMARY KEY AUTOINCREMENT,  -- 主键 ID
    "endpoint"      TEXT,                               -- 请求路径（如/api/v1/projects）
    "http_method"   TEXT,                               -- HTTP 方法（POST）
    "request_body"  TEXT,                               -- 请求体（JSON）
    "response_code" INTEGER,                            -- 响应状态码（200/404/500等）
    "response_body" TEXT,                               -- 响应体（JSON）
    "client_ip"     TEXT,                               -- 客户端 IP 地址
    "duration_ms"   INTEGER,                            -- 请求耗时（毫秒）
    "created_at"    TEXT NOT NULL DEFAULT (datetime('now'))  -- 记录时间
);

-- ==================== API 密钥表 ====================
CREATE TABLE IF NOT EXISTS "api_key" (
    "id"           INTEGER PRIMARY KEY AUTOINCREMENT,  -- 主键 ID
    "client_id"    TEXT NOT NULL,                      -- 客户端唯一标识
    "client_name"  TEXT,                               -- 客户端名称（如某检测公司）
    "api_key_hash" TEXT NOT NULL,                      -- API Key SHA256 哈希
    "status"       INTEGER NOT NULL DEFAULT 1,         -- 状态（1=启用, 0=禁用）
    "expires_at"   TEXT,                               -- 过期时间
    "created_by"   TEXT,                               -- 创建人
    "created_at"   TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间
    "updated_at"   TEXT                                -- 更新时间
);
