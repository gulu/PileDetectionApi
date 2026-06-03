# PileDetection API 接口文档

> 基桩超声波透射检测数据采集接口平台 — 第三方集成接口文档
>
> 版本：v1.1 | 更新日期：2026-05-12

---

## 目录

1. [接口规范](#1-接口规范)
2. [认证接入](#2-认证接入)
3. [项目管理](#3-项目管理)
4. [基桩管理](#4-基桩管理)
5. [剖面统计](#5-剖面统计)
6. [测点数据](#6-测点数据)
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
| clientId | client_20260518_5ef0c298 |
| apiKey | pile_sk_--8tZnckCaZ3en0S-7ZrYjEfqtU3WW_Z_F1hhk2_E36hMYFGSTtKSFc55qAYwOBa |
| **projectId** | 6b5e01fc-93bf-4375-ae66-dc249ba8bd2c |
| **projectName** | 智博联数据同步测试 |

### 1.2 通用约定

- **请求方式**：所有业务接口统一使用 **POST**
- **字符编码**：UTF-8
- **Content-Type**：`application/json; charset=utf-8`
- **认证方式**：`Authorization: Bearer {jwt_token}`
- **ID 类型**：所有主键和外键均为 **GUID 字符串**（如 `"a1b2c3d4-e5f6-7890-abcd-ef1234567890"`）
- **时间格式**：ISO 8601（如 `2026-05-11T10:00:00Z`）
- **分页参数**：通过 Request Body 传入 `page`（从 1 开始）、`pageSize`（默认 20，最大 100）

### 1.3 通用响应结构

所有接口统一返回以下格式：

```json
{
  "code": 200,
  "message": "成功",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-05-11T10:00:00.0000000Z"
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `code` | int | HTTP 状态码 |
| `message` | string | 提示信息 |
| `data` | object | 业务数据 |
| `errors` | object/null | 错误详情（失败时返回） |
| `timestamp` | string | 服务器时间戳 |

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

### 2.1 获取 API Key（先联系管理员）

调用所有业务接口前，需先向系统管理员申请 API Key。管理员通过管理端接口生成 Key 后，会提供以下信息：

| 参数 | 示例 | 说明 |
|------|------|------|
| `clientId` | `client_20260511_abc12345` | 客户端唯一标识 |
| `apiKey` | `pile_sk_xxxxxxxx...` | 用于换取 Token（仅创建时可见） |

### 2.2 换取 JWT Token

```
POST /api/v1/auth/tokenByUserid
```

**Request Body：**

```json
{
  "apiKey": "",
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
    "expiresAt": "2026-05-13T10:00:00Z",
    "tokenType": "Bearer"
  }
}
```

| 字段 | 类型 | 说明 |
|------|------|------|
| `token` | string | JWT Token（有效期内使用） |
| `expiresAt` | string | 过期时间（默认 24 小时） |
| `tokenType` | string | 固定 `Bearer` |

### 2.3 使用 Token 调用业务接口

所有业务接口在 HTTP Header 中传入：

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

---

## 3. 项目管理

### 3.1 创建项目

```
POST /api/v1/projects
```

**Request Body：**

```json
{
  "projectName": "碗窑岭大桥",
  "projectNo": "PROJ-2026-001",
  "projectLocation": "浙江省温州市",
  "projectManager": "张三",
  "projectDesc": "桥梁基桩检测"
}
```

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `projectName` | string | 是 | 项目名称 |
| `projectNo` | string | 否 | 项目编号 |
| `projectLocation` | string | 否 | 项目地点 |
| `projectManager` | string | 否 | 项目负责人 |
| `projectDesc` | string | 否 | 项目描述 |

**Response（201）：**

```json
{
  "code": 201,
  "message": "创建成功",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
    "projectName": "碗窑岭大桥",
    "projectNo": "PROJ-2026-001",
    "projectLocation": "浙江省温州市",
    "projectManager": "张三",
    "projectDesc": "桥梁基桩检测",
    "createdAt": "2026-05-12T10:00:00Z",
    "updatedAt": null
  }
}
```

### 3.2 查询项目列表

```
POST /api/v1/projects/list
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
        "createdAt": "2026-05-12T10:00:00Z",
        "updatedAt": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1
  }
}
```

### 3.3 查询项目详情

```
POST /api/v1/projects/{id}
```

**路径参数：**

| 参数 | 类型 | 说明 |
|------|------|------|
| `id` | guid | 项目 ID（如 `a1b2c3d4-...-ef123456789001`） |

**Response：**

```json
{
  "code": 200,
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
    "projectName": "碗窑岭大桥",
    "projectNo": "PROJ-2026-001",
    "projectLocation": "浙江省温州市",
    "projectManager": "张三",
    "projectDesc": "桥梁基桩检测",
    "pileCount": 5,
    "createdAt": "2026-05-12T10:00:00Z"
  }
}
```

### 3.4 更新项目

```
POST /api/v1/projects/{projectId}/update
```

**Request Body（所有字段可选，只传需要更新的）：**

```json
{
  "projectName": "碗窑岭大桥（修改后）",
  "projectLocation": "浙江省温州市鹿城区"
}
```

**Response：**

```json
{
  "code": 200,
  "message": "成功",
  "data": {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef123456789001",
    "projectName": "碗窑岭大桥（修改后）",
    "projectNo": "PROJ-2026-001",
    "projectLocation": "浙江省温州市鹿城区",
    "projectManager": "张三",
    "projectDesc": "桥梁基桩检测",
    "createdAt": "2026-05-12T10:00:00Z",
    "updatedAt": "2026-05-12T11:00:00Z"
  }
}
```

### 3.5 删除项目（软删除）

```
POST /api/v1/projects/{projectId}/delete
```

> 软删除，数据仍保留在数据库中，但不再出现在查询结果中。

**Response：**

```json
{
  "code": 200,
  "message": "删除成功",
  "data": {}
}
```

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
  "pourDate": "2026-03-15T00:00:00Z",
  "testDate": "2026-03-18T00:00:00Z",
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
    "pourDate": "2026-03-15T00:00:00Z",
    "testDate": "2026-03-18T00:00:00Z",
    "testStandard": "JGJ 106-2014",
    "instrumentModel": "RSM-SY7",
    "instrumentSn": "SN2026001",
    "tester": "李四",
    "testerCertNo": "JC-2026-001",
    "createdAt": "2026-05-12T10:00:00Z",
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
        "createdAt": "2026-05-12T10:00:00Z"
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
    "pourDate": "2026-03-15T00:00:00Z",
    "testDate": "2026-03-18T00:00:00Z",
    "testStandard": "JGJ 106-2014",
    "instrumentModel": "RSM-SY7",
    "instrumentSn": "SN2026001",
    "tester": "李四",
    "testerCertNo": "JC-2026-001",
    "integrityCategory": 1,
    "createdAt": "2026-05-12T10:00:00Z",
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
      "reportDate": "2026-05-11T00:00:00Z",
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
    "updatedAt": "2026-05-12T11:00:00Z"
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
    "psd": 85.3
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
      "psd": 85.3
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
      "psd": 85.3
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
  "amplitude": 132.0
}
```

### 6.5 删除单条测点数据

```
POST /api/v1/piles/{pileId}/measurements/{id}/delete
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
  "reportDate": "2026-05-11T00:00:00Z",
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
    "reportDate": "2026-05-11T00:00:00Z",
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
  "reportDate": "2026-05-11T00:00:00Z",
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
    "reportDate": "2026-05-11T00:00:00Z",
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

> 以下接口需要提供 **Master Key**（管理员持有，非第三方调用）。

### 9.1 生成新 API Key

```
POST /api/v1/admin/api-keys
Headers: X-Master-Key: admin-master-key-2026-pile-detection
```

**Request Body：**

```json
{
  "clientName": "某检测公司",
  "expireDays": 365
}
```

**Response：**

```json
{
  "code": 200,
  "data": {
    "clientId": "client_20260511_abc12345",
    "apiKey": "pile_sk_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "clientName": "某检测公司",
    "expiresAt": "2027-05-12T10:00:00Z"
  }
}
```

> **注意**：`apiKey` 明文仅在创建时返回一次，请妥善保存。

### 9.2 列出所有 API Key

```
POST /api/v1/admin/api-keys/list
Headers: X-Master-Key: admin-master-key-2026-pile-detection
```

**Response：**

```json
{
  "code": 200,
  "data": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890ff",
      "clientId": "client_20260511_abc12345",
      "clientName": "某检测公司",
      "status": 1,
      "expiresAt": "2027-05-12T10:00:00Z",
      "createdAt": "2026-05-12T10:00:00Z"
    }
  ]
}
```

### 9.3 启用/禁用 API Key

```
POST /api/v1/admin/api-keys/{id}/toggle
Headers: X-Master-Key: admin-master-key-2026-pile-detection
```

**Response：**

```json
{
  "code": 200,
  "message": "状态已切换",
  "data": {}
}
```

---

## 10. 附录

### 10.1 通用响应结构

```json
{
  "code": 200,
  "message": "成功",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-05-12T10:00:00.0000000Z"
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
  "timestamp": "2026-05-12T10:00:00.0000000Z"
}
```

### 10.3 错误响应

```json
{
  "code": 401,
  "message": "API Key 无效或已过期",
  "data": null,
  "errors": { "detail": "API Key 无效" },
  "timestamp": "2026-05-12T10:00:00.0000000Z"
}
```

```json
{
  "code": 500,
  "message": "服务器内部错误",
  "data": null,
  "errors": { "detail": "NullReferenceException: Object reference not set..." },
  "timestamp": "2026-05-12T10:00:00.0000000Z"
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

日志文件路径（通过 `appsettings.json` 配置）：`Logs/pile-{Date}.log`

**日志输出示例：**

```
[10:00:00 INF] [API] POST /api/v1/projects/list → 200 (45ms)
  ClientIp: 192.168.1.100
  Request: {"page":1,"pageSize":20}
  Response: {"code":200,"data":{"items":[...],"page":1,"pageSize":20,"totalCount":5}}
```

### 10.5 实体关系图

数据库 ER 图请参考 [er-diagram.html](./er-diagram.html) 或 [er-diagram.mmd](./er-diagram.mmd)。

``` text
project_info ──1:N──→ pile_info ──1:N──→ profile_stat
                               ├──1:N──→ measurement_data
                               └──1:1──→ pile_report
project_info ──1:N──→ project_report
api_log (独立表)           api_key (独立表)
```

### 10.6 调用时序

``` text
第三方系统                    PileDetection API
    │                              │
    │  1. 申请 API Key (管理)       │
    │───── 联系管理员 →─────────────│
    │←──── 获取 clientId + apiKey ─│
    │                              │
    │  2. 换取 Token (无需认证)     │
    │──── POST /api/v1/auth/token ─│
    │←─── { token, expiresAt } ────│
    │                              │
    │  3. 调用业务接口 (全部 POST)  │
    │──── POST /projects/list ─────│  Authorization: Bearer xxx
    │←─── { data: [...] } ────────│
    │                              │
    │  4. Token 过期后重复步骤 2    │
    │                              │
```

---

> 文档完 — 如有疑问请联系系统管理员。
