# 基桩超声波透射检测数据采集接口平台

> 第三方集成接口文档
>
> 版本：v1.2 | 更新日期：2026-06-03

---

## 目录

1. [接口规范](#1-接口规范)
2. [认证接入](#2-认证接入)
3. [项目管理](#3-项目管理)
   - 3.1[查询当前账号有处理权限的项目列表](#31-查询当前账号有处理权限的项目列表)
4. [基桩管理](#4-基桩管理)
5. [剖面统计](#5-剖面统计)
6. [测点数据](#6-测点数据)
   - 6.6[查询原始波形矩阵](#66-查询原始波形矩阵)
7. [报告管理](#7-报告管理)
8. [数据导出](#8-数据导出)
9. [管理端 API Key 管理](#9-管理端-api-key-管理)
10. [附录](#10-附录)

---

## 1. 接口规范

### 1.1 基础地址

| 环境 | 地址 |
|------|------|
| 测试环境 | `http://121.199.16.6:3007` |
| clientId | ZBL |
| clientName | 智博联 |

### 1.2 通用约定

- **请求方式**：所有业务接口统一使用 **POST**
- **字符编码**：UTF-8
- **Content-Type**：`application/json; charset=utf-8`
- **认证方式**：`Authorization: Bearer {jwt_token}`
- **ID 类型**：所有主键和外键均为 **GUID 字符串**（如 `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"`）
- **时间格式**：`yyyy-MM-dd HH:mm:ss`（如 `2026-05-11 10:00:00`）
- **分页参数**：通过 Request Body 传入 `page`（从 1 开始）、`pageSize`（默认 20，最大 100）

### 1.3 通用响应结构

所有接口统一返回以下格式：

```json
{
  "code": 200,
  "message": "成功",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-05-11 10:00:00"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `code` | int | HTTP 状态码 |
| `message` | string | 提示信息 |
| `data` | object | 业务数据 |
| `errors` | object/null | 错误详情（失败时返回） |
| `timestamp` | string | 服务器时间戳，格式 `yyyy-MM-dd HH:mm:ss` |

### 1.4 HTTP 状态码说明

| 状态码 | 含义 |
|--------|------|
| 200 | 成功 |
| 201 | 创建成功 |
| 400 | 参数错误 |
| 401 | 未授权（Token 无效/过期） |
| 404 | 资源不存在 |
| 409 | 数据重复 |
| 500 | 服务器内部错误 |

---

## 2. 认证接入

> 第三方系统对接时，管理员会分配一个唯一的 **clientId**（用户账号）和对应的 **clientName**（登录名）。

### 2.1 获取账号信息（先联系管理员）

调用所有业务接口前，需先向系统管理员申请接入账号。管理员通过管理端生成账号后，会提供以下信息：

| 参数 | 示例 | 说明 |
|------|------|------|
| `clientId`（用户ID） | `ZBL` | 客户端唯一标识，相当于"用户ID" |
| `clientName` | `智博联...` | 用户名 |

### 2.2 登录换取 JWT Token

使用分配的 ClientId 和 clientName 后 ，换取后续接口调用所需的 JWT Token。


```
POST /api/v1/auth/tokenByUserid
```

**Request Body：**

```json
{
  "apiKey": "string",
  "clientId": "ZBL",
  "clientName": "智博联"
}
```

**Response：**

```json
{
  "code": 200,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ",
    "expiresAt": "2026-05-13 10:00:00",
    "tokenType": "Bearer"
  }
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `token` | string | JWT Token（有效期内使用） |
| `expiresAt` | string | 过期时间（默认 24 小时，格式 `yyyy-MM-dd HH:mm:ss`） |
| `tokenType` | string | 固定 `Bearer` |

### 2.3 使用 Token 调用业务接口

所有业务接口在 HTTP Header 中传入：

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 3. 项目管理



### 3.6 查询当前账号有处理权限的项目列表

> 根据登录账号（`clientId`）过滤，仅返回该账号被管理员授权可以处理的项目。

```
POST /api/v1/projects/permitted-list
```

**Request Body：**

```json
{
  "page": 1,
  "pageSize": 20,
  "keyword": "碗窑岭"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `page` | int | 否 | 页码（默认 1） |
| `pageSize` | int | 否 | 每页条数（默认 20） |
| `keyword` | string | 否 | 关键字搜索（按项目名称模糊匹配） |

**Response：**

```json
{
  "code": 200,
  "data": {
    "items": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
        "projectName": "碗窑岭大桥",
        "projectNo": "PROJ-2026-001",
        "projectLocation": "浙江省温州市",
        "projectManager": "张三",
        "projectDesc": "桥梁基桩检测",
        "createdAt": "2026-05-12 10:00:00",
        "updatedAt": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1
  }
}
```

> **说明：** 此接口会自动识别当前登录的账号，只返回该账号有处理权限的项目列表。未授权访问的项目不会出现在结果中。如需授权/管理权限，请联系系统管理员。

---

## 4. 基桩管理

### 4.1 创建基桩

```
POST /api/v1/projects/{projectId}/piles
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `projectId` | guid | 所属项目 ID |

**Request Body：**

```json
{
  "pileName": "右11",
  "designLength": 11.7,
  "designDiameter": 2000,
  "designStrength": "C25",
  "pourDate": "2026-03-15 00:00:00",
  "testDate": "2026-03-18 00:00:00",
  "testStandard": "JGJ 106-2014",
  "instrumentModel": "RSM-SY7",
  "instrumentSn": "SN2026001",
  "tester": "李四",
  "testerCertNo": "JC-2026-001"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `pileName` | string | 是 | 桩号 |
| `designLength` | double | 否 | 设计桩长（m） |
| `designDiameter` | int | 否 | 设计桩径（mm） |
| `designStrength` | string | 否 | 设计强度等级 |
| `pourDate` | string | 否 | 浇筑日期 |
| `testDate` | string | 否 | 检测日期 |
| `testStandard` | string | 否 | 检测标准 |
| `instrumentModel` | string | 否 | 仪器型号 |
| `instrumentSn` | string | 否 | 仪器编号 |
| `tester` | string | 否 | 检测人 |
| `testerCertNo` | string | 否 | 检测人证书号 |

**Response（201）：**

```json
{
  "code": 201,
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
    "projectId": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
    "projectName": "碗窑岭大桥",
    "pileName": "右11",
    "designLength": 11.7,
    "designDiameter": 2000,
    "designStrength": "C25",
    "pourDate": "2026-03-15 00:00:00",
    "testDate": "2026-03-18 00:00:00",
    "testStandard": "JGJ 106-2014",
    "instrumentModel": "RSM-SY7",
    "instrumentSn": "SN2026001",
    "tester": "李四",
    "testerCertNo": "JC-2026-001",
    "createdAt": "2026-05-12 10:00:00",
    "updatedAt": null
  }
}
```

### 4.2 查询项目下基桩列表

```
POST /api/v1/projects/{projectId}/piles/list
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `projectId` | guid | 所属项目 ID |

**Request Body：**

```json
{
  "page": 1,
  "pageSize": 20
}
```

**Response：**

```json
{
  "code": 200,
  "data": {
    "items": [
      {
        "id": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
        "pileName": "右11",
        "designLength": 11.7,
        "designDiameter": 2000,
        "integrityCategory": 1,
        "createdAt": "2026-05-12 10:00:00"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1
  }
}
```

### 4.3 查询单桩完整信息

```
POST /api/v1/piles/{id}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `id` | guid | 基桩 ID |

**Response：**

```json
{
  "code": 200,
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
    "projectId": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
    "projectName": "碗窑岭大桥",
    "pileName": "右11",
    "designLength": 11.7,
    "designDiameter": 2000,
    "designStrength": "C25",
    "pourDate": "2026-03-15 00:00:00",
    "testDate": "2026-03-18 00:00:00",
    "testStandard": "JGJ 106-2014",
    "instrumentModel": "RSM-SY7",
    "instrumentSn": "SN2026001",
    "tester": "李四",
    "testerCertNo": "JC-2026-001",
    "integrityCategory": 1,
    "createdAt": "2026-05-12 10:00:00",
    "updatedAt": null,
    "profileStats": [
      {
        "id": "c3d4e5f6-a7b8-9012-cdef-1234567890ab",
        "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
        "profile": "1-2",
        "distance": 1022,
        "avgVelocity": 4.026,
        "avgAmplitude": 128.979
      }
    ],
    "measurements": [
      {
        "id": "d4e5f6a7-b8c9-0123-defa-1234567890bc",
        "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
        "profile": "1-2",
        "depth": 0.5,
        "soundVelocity": 4.12,
        "amplitude": 130.5,
        "soundTime": 248.0,
        "psd": 85.3
      }
    ],
    "report": {
      "id": "e5f6a7b8-c9d0-1234-efab-2345678901cd",
      "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
      "reportNo": "BG-2026-001",
      "reportDate": "2026-05-11 00:00:00",
      "integrityCategory": 1,
      "avgVelocity": 4.15,
      "criticalVelocity": 3.42,
      "avgAmplitude": 126.8,
      "criticalAmplitude": 115.2,
      "conclusion": "桩身完整性类别为Ⅰ类桩。"
    }
  }
}
```

### 4.4 更新基桩

```
POST /api/v1/piles/{id}/update
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `id` | guid | 基桩 ID |

**Request Body（所有字段可选）：**

```json
{
  "pileName": "右11-复测",
  "integrityCategory": 1
}
```

**Response：**

```json
{
  "code": 200,
  "data": {
    "id": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
    "pileName": "右11-复测",
    "integrityCategory": 1,
    "updatedAt": "2026-05-12 11:00:00"
  }
}
```

### 4.5 删除基桩（软删除）

```
POST /api/v1/piles/{id}/delete
```

**Response：**

```json
{ "code": 200, "message": "删除成功", "data": {} }
```

---

## 5. 剖面统计

> 每个基桩包含多个检测剖面（如 1-2、1-3、1-4 等），每个剖面有一组统计指标。

### 5.1 批量添加剖面统计

```
POST /api/v1/piles/{pileId}/profiles
```

**Request Body（数组）：**

```json
[
  {
    "profile": "1-2",
    "distance": 1022,
    "maxVelocity": 4.5,
    "minVelocity": 3.8,
    "avgVelocity": 4.026,
    "stdVelocity": 0.25,
    "cvVelocity": 6.21,
    "criticalVelocity": 3.5,
    "maxAmplitude": 135.2,
    "minAmplitude": 120.1,
    "avgAmplitude": 128.979,
    "stdAmplitude": 5.12,
    "cvAmplitude": 3.97,
    "criticalAmplitude": 118.0
  },
  {
    "profile": "1-3",
    "distance": 1506,
    "avgVelocity": 4.133,
    "avgAmplitude": 124.893
  }
]
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `profile` | string | 是 | 剖面名称（如 `1-2`） |
| `distance` | double | 否 | 测距（mm） |
| `maxVelocity` | double | 否 | 最大声速（km/s） |
| `minVelocity` | double | 否 | 最小声速（km/s） |
| `avgVelocity` | double | 否 | 平均声速（km/s） |
| `stdVelocity` | double | 否 | 声速标准差 |
| `cvVelocity` | double | 否 | 声速变异系数（%） |
| `criticalVelocity` | double | 否 | 声速临界值（km/s） |
| `maxAmplitude` | double | 否 | 最大波幅（dB） |
| `minAmplitude` | double | 否 | 最小波幅（dB） |
| `avgAmplitude` | double | 否 | 平均波幅（dB） |
| `stdAmplitude` | double | 否 | 波幅标准差 |
| `cvAmplitude` | double | 否 | 波幅变异系数（%） |
| `criticalAmplitude` | double | 否 | 波幅临界值（dB） |

**Response（201）：**

```json
{
  "code": 201,
  "data": [
    {
      "id": "c3d4e5f6-a7b8-9012-cdef-1234567890ab",
      "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
      "profile": "1-2",
      "distance": 1022,
      "maxVelocity": 4.5,
      "minVelocity": 3.8,
      "avgVelocity": 4.026,
      "stdVelocity": 0.25,
      "cvVelocity": 6.21,
      "criticalVelocity": 3.5,
      "maxAmplitude": 135.2,
      "minAmplitude": 120.1,
      "avgAmplitude": 128.979,
      "stdAmplitude": 5.12,
      "cvAmplitude": 3.97,
      "criticalAmplitude": 118.0
    }
  ]
}
```

### 5.2 查询剖面列表

```
POST /api/v1/piles/{pileId}/profiles/list
```

**Response：**

```json
{
  "code": 200,
  "data": [
    {
      "id": "c3d4e5f6-a7b8-9012-cdef-1234567890ab",
      "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
      "profile": "1-2",
      "distance": 1022,
      "avgVelocity": 4.026,
      "avgAmplitude": 128.979
    }
  ]
}
```

### 5.3 更新剖面统计

```
POST /api/v1/piles/{pileId}/profiles/{id}/update
```

**Request Body：**

```json
{
  "avgVelocity": 4.15,
  "criticalVelocity": 3.55
}
```

### 5.4 删除剖面统计

```
POST /api/v1/piles/{pileId}/profiles/{id}/delete
```

---

## 6. 测点数据

> 每个剖面的每个深度位置记录一组测值。

### 6.1 批量添加测点数据

```
POST /api/v1/piles/{pileId}/measurements
```

**Request Body（数组，建议每批 ≤ 200 条）：**

```json
[
  {
    "profile": "1-2",
    "depth": 0.5,
    "soundVelocity": 4.12,
    "amplitude": 130.5,
    "soundTime": 248.0,
    "psd": 85.3,
    "rawWaveform": {
      "samplingRate": 500.0,
      "pointCount": 512,
      "storageType": 1,
      "rawPointsJson": "[0.12, -0.45, 1.2, -0.87, 0.33, ...]"
    }
  },
  {
    "profile": "1-2",
    "depth": 1.0,
    "soundVelocity": 4.08,
    "amplitude": 128.2,
    "soundTime": 250.5,
    "psd": 82.1
  }
]
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `profile` | string | 是 | 所属剖面名称 |
| `depth` | double | 是 | 深度（m） |
| `soundVelocity` | double | 否 | 声速（km/s） |
| `amplitude` | double | 否 | 波幅（dB） |
| `soundTime` | double | 否 | 声时（us） |
| `psd` | double | 否 | PSD 值 |
| `rawWaveform` | object | 否 | 原始波形矩阵（可选，有则写入） |

**`rawWaveform` 子字段：**

| 字段 | 类型 | 必填 | 默认值 | 说明 |
|------|------|------|--------|------|
| `samplingRate` | double | 否 | 500.0 | 采样频率（kHz） |
| `pointCount` | int | 否 | 512 | 采样点数（如 512、1024） |
| `storageType` | int | 否 | 1 | 存储模式：1=数据库内联 JSON, 2=外部文件 |
| `rawPointsJson` | string | 否 | — | 模式1：波形数据 JSON 数组字符串 |
| `filePath` | string | 否 | — | 模式2：外部 `.npy` / `.h5` 文件路径 |

**Response（201）：**

```json
{
  "code": 201,
  "data": [
    {
      "id": "d4e5f6a7-b8c9-0123-defa-1234567890bc",
      "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
      "profile": "1-2",
      "depth": 0.5,
      "soundVelocity": 4.12,
      "amplitude": 130.5,
      "soundTime": 248.0,
      "psd": 85.3,
      "hasWaveform": true
    },
    {
      "id": "e5f6a7b8-c9d0-1234-efab-2345678901cd",
      "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
      "profile": "1-2",
      "depth": 1.0,
      "soundVelocity": 4.08,
      "amplitude": 128.2,
      "soundTime": 250.5,
      "psd": 82.1,
      "hasWaveform": false
    }
  ]
}
```

### 6.2 查询测点数据

```
POST /api/v1/piles/{pileId}/measurements/list
```

**Request Body：**

```json
{
  "minDepth": 0,
  "maxDepth": 10
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `minDepth` | double | 否 | 最小深度过滤 |
| `maxDepth` | double | 否 | 最大深度过滤 |

**Response：**

```json
{
  "code": 200,
  "data": [
    {
      "id": "d4e5f6a7-b8c9-0123-defa-1234567890bc",
      "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
      "profile": "1-2",
      "depth": 0.5,
      "soundVelocity": 4.12,
      "amplitude": 130.5,
      "soundTime": 248.0,
      "psd": 85.3,
      "hasWaveform": true
    }
  ]
}
```

### 6.3 按剖面查询测点

```
POST /api/v1/piles/{pileId}/measurements/profile/{profileName}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `profileName` | string | 剖面名称（如 `1-2`） |

### 6.4 更新单条测点数据

```
POST /api/v1/piles/{pileId}/measurements/{id}/update
```

**Request Body：**

```json
{
  "soundVelocity": 4.2,
  "amplitude": 132.0,
  "rawWaveform": {
    "samplingRate": 500.0,
    "pointCount": 1024,
    "storageType": 1,
    "rawPointsJson": "[0.15, -0.42, 1.18, ...]"
  }
}
```

> `rawWaveform` 为可选字段。若传入则对该测点 upsert（更新或插入）波形矩阵数据。

### 6.5 删除单条测点数据

```
POST /api/v1/piles/{pileId}/measurements/{id}/delete
```

### 6.6 查询原始波形矩阵

> 获取单条测点记录的原始波形矩阵数据。**每条约 512–1024 个浮点采样值，按需调用**。

```
POST /api/v1/piles/{pileId}/measurements/{id}/waveform
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `pileId` | guid | 基桩 ID |
| `id` | guid | 测点 ID（measurement_data_id） |

**Response（200）：**

```json
{
  "code": 200,
  "data": {
    "measurementDataId": "d4e5f6a7-b8c9-0123-defa-1234567890bc",
    "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
    "samplingRate": 500.0,
    "pointCount": 512,
    "storageType": 1,
    "rawPointsJson": "[0.12, -0.45, 1.2, -0.87, 0.33, ...]",
    "filePath": null,
    "createdAt": "2026-06-03 10:00:00"
  }
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `measurementDataId` | guid | 测点 ID |
| `pileInfoId` | guid | 基桩 ID |
| `samplingRate` | double | 采样频率（kHz） |
| `pointCount` | int | 采样点数 |
| `storageType` | int | 存储模式：1=内联 JSON, 2=外部文件 |
| `rawPointsJson` | string/null | 模式1：波形数据 JSON 数组字符串 |
| `filePath` | string/null | 模式2：外部文件路径 |
| `createdAt` | string | 创建时间 |

**无波形数据时（404）：**

```json
{
  "code": 404,
  "message": "该测点无原始波形矩阵数据",
  "data": null
}
```

---

## 7. 报告管理

### 7.1 创建/更新单桩报告

```
POST /api/v1/piles/{pileId}/report
```

> POST 幂等：如果已存在报告则更新，不存在则创建。

**Request Body：**

```json
{
  "reportNo": "BG-2026-001",
  "reportDate": "2026-05-11 00:00:00",
  "integrityCategory": 1,
  "avgVelocity": 4.15,
  "criticalVelocity": 3.42,
  "avgAmplitude": 126.8,
  "criticalAmplitude": 115.2,
  "conclusion": "桩身完整性类别为Ⅰ类桩。"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `reportNo` | string | 否 | 报告编号 |
| `reportDate` | string | 否 | 报告日期 |
| `integrityCategory` | int | 否 | 完整性类别（1=Ⅰ类, 2=Ⅱ类, 3=Ⅲ类, 4=Ⅳ类） |
| `avgVelocity` | double | 否 | 平均声速（km/s） |
| `criticalVelocity` | double | 否 | 临界声速（km/s） |
| `avgAmplitude` | double | 否 | 平均波幅（dB） |
| `criticalAmplitude` | double | 否 | 临界波幅（dB） |
| `conclusion` | string | 否 | 检测结论 |

**Response：**

```json
{
  "code": 200,
  "data": {
    "id": "e5f6a7b8-c9d0-1234-efab-2345678901cd",
    "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
    "reportNo": "BG-2026-001",
    "reportDate": "2026-05-11 00:00:00",
    "integrityCategory": 1,
    "avgVelocity": 4.15,
    "criticalVelocity": 3.42,
    "avgAmplitude": 126.8,
    "criticalAmplitude": 115.2,
    "conclusion": "桩身完整性类别为Ⅰ类桩。"
  }
}
```

### 7.2 查询单桩报告

```
POST /api/v1/piles/{pileId}/report/detail
```

**Response：**

```json
{
  "code": 200,
  "data": {
    "id": "e5f6a7b8-c9d0-1234-efab-2345678901cd",
    "pileInfoId": "b2c3d4e5-f6a7-8901-bcde-f123456789012",
    "reportNo": "BG-2026-001",
    "integrityCategory": 1,
    "avgVelocity": 4.15,
    "conclusion": "桩身完整性类别为Ⅰ类桩。"
  }
}
```

### 7.3 创建/更新项目报告

```
POST /api/v1/projects/{projectId}/report
```

**Request Body：**

```json
{
  "reportNo": "XM-BG-2026-001",
  "reportDate": "2026-05-11 00:00:00",
  "conclusion": "本次检测共1根桩，完整性合格。",
  "integritySummary": "Ⅰ类桩: 1根, Ⅱ类桩: 0根, Ⅲ类桩: 0根, Ⅳ类桩: 0根"
}
```

**Response：**

```json
{
  "code": 200,
  "data": {
    "id": "f6a7b8c9-d0e1-2345-fabc-3456789012de",
    "projectId": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
    "reportNo": "XM-BG-2026-001",
    "reportDate": "2026-05-11 00:00:00",
    "conclusion": "本次检测共1根桩，完整性合格。",
    "integritySummary": "Ⅰ类桩: 1根, Ⅱ类桩: 0根, Ⅲ类桩: 0根, Ⅳ类桩: 0根"
  }
}
```

### 7.4 查询项目报告

```
POST /api/v1/projects/{projectId}/report/detail
```

---

## 8. 数据导出

### 8.1 导出项目汇总报告（xlsx）

```
POST /api/v1/projects/{id}/export
```

**Response：** 返回 `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` 格式的 Excel 文件下载。

### 8.2 导出单桩报告（xlsx）

```
POST /api/v1/piles/{id}/export
```

---

## 9. 管理端 API Key 管理

> 验证 **Master Key**（管理员使用，非第三方调用）。

---

## 10. 附录

### 10.1 通用响应结构

```json
{
  "code": 200,
  "message": "成功",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-05-12 10:00:00"
}
```

### 10.2 分页响应

```json
{
  "code": 200,
  "data": {
    "items": [ ... ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 50
  },
  "timestamp": "2026-05-12 10:00:00"
}
```

### 10.3 错误响应

```json
{
  "code": 401,
  "message": "API Key 无效或已过期",
  "data": null,
  "errors": { "detail": "API Key 无效" },
  "timestamp": "2026-05-12 10:00:00"
}
```

```json
{
  "code": 500,
  "message": "服务器内部错误",
  "data": null,
  "errors": { "detail": "NullReferenceException: Object reference not set..." },
  "timestamp": "2026-05-12 10:00:00"
}
```

### 10.4 日志说明

每次 API 调用自动记录以下信息到日志文件和 `api_log` 表：

| 字段 | 说明 |
|------|------|
| 请求路径 | `POST /api/v1/projects/list` |
| 请求方法 | POST |
| 请求体 | JSON 完整内容（最长 8000 字符） |
| 响应体 | JSON 完整内容（最长 8000 字符） |
| 响应状态码 | 200 / 201 / 404 / 500 等 |
| 客户端 IP | 访问来源 IP |
| 耗时 | 请求处理时间（毫秒） |



### 10.5调用时序

``` text
第三方系统                    PileDetection API
    │                              │
    │  1. 申请账号 (管理)           │
    │───── 联系管理员 →─────────────│
    │←──── 获取 clientId + apiKey ─│
    │                              │
    │  2. 登录换取 Token            │
    │──── POST /api/v1/auth/token ─│
    │←─── { token, expiresAt } ────│
    │                              │
    │  3. 查询有权处理的项目         │
    │──── POST /projects/permitted-list ─  Authorization: Bearer xxx
    │←─── { items: [...] } ────────│
    │                              │
    │  4. 调用其他业务接口           │
    │──── POST /projects/list ─────│  Authorization: Bearer xxx
    │←─── { data: [...] } ────────│
    │                              │
    │  5. Token 过期后重复步骤 2    │
    │                              │
```

---

> 文档完 — 如有疑问请联系系统管理员。
