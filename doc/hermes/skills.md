# Hermes Skills 技能注册

> Hermes 技能存在 `~/.hermes/skills/` 目录下，每个技能一个独立的 `SKILL.md` 文件。
> 本文件作为项目技能注册表，记录本项目中注册的 Hermes 技能名称和用途，
> 也作为新技能的触发器条件参考。

## 注册的技能

| 技能名 | 触发条件 | 路径 | 状态 |
|--------|----------|------|------|
| （项目专属技能待创建） | — | — | ⏳ |

## 计划注册的技能

| 技能名 | 用途 | 优先级 |
|--------|------|--------|
| .net-core-freesql-scaffold | .NET Core 8 + FreeSql 项目脚手架搭建（多数据库配置、AutoMapper、FluentValidation、JWT、Serilog） | 高 |
| json-api-crud | 项目/基桩/剖面/测点/报告的通用 JSON API CRUD 实现模式 | 高 |
| xlsx-export-closedxml | ClosedXML 实现多 Sheet xlsx 导出（单桩报告、项目汇总） | 高 |
| jwt-auth-setup | JWT Bearer Token 认证实现（Token 签发 + Middleware 鉴权） | 高 |
| serilog-dual-logging | Serilog 文件+数据库表双通道日志配置 | 中 |

## 触发器条件参考

| 用户表述 | 匹配技能 |
|----------|----------|
| "创建项目" / "新建 API" / "脚手架" / "初始化" | .net-core-freesql-scaffold |
| "上传数据" / "接收 JSON" / "CRUD" / "第三方调用" | json-api-crud |
| "导出 Excel" / "导出 xlsx" / "生成报告" / "导出" | xlsx-export-closedxml |
| "认证" / "鉴权" / "Token" / "JWT" / "安全" | jwt-auth-setup |
| "日志" / "Serilog" / "记录日志" | serilog-dual-logging |
