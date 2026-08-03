# 插件后端接口约定

## 请求

```http
GET /api/v1/groups/itcc-l2/unassigned-tickets
Accept: application/json
```

插件不向后端传 Helix 查询条件。后端固定筛选：

```text
ASGRP = "IT Control Center L2"
AND ASCHG 为空
AND Status 属于未结束状态
```

Token、apikey、Helix字段映射和分页全部由后端负责。

## 成功响应

```json
{
  "group": "IT Control Center L2",
  "generatedAt": "2026-08-03T14:32:00+02:00",
  "total": 2,
  "counts": {
    "INC": 1,
    "WO": 1
  },
  "tickets": [
    {
      "id": "INC000123456",
      "type": "INC",
      "summary": "Q站客户端无法提交数据",
      "priority": "High",
      "status": "Assigned",
      "createdAt": "2026-08-03T13:56:00+02:00",
      "groupAssignedAt": null,
      "url": "https://helix.example.com/ticket/INC000123456"
    }
  ]
}
```

## 字段说明

| 字段 | 必需 | 说明 |
| --- | --- | --- |
| `group` | 是 | 监控组显示名 |
| `generatedAt` | 是 | 后端生成当前快照的时间 |
| `tickets` | 是 | 精简后的工单数组 |
| `id` | 是 | INC或WO单号 |
| `type` | 是 | 仅允许 `INC`、`WO` |
| `summary` | 是 | 列表摘要，不应包含完整Description |
| `priority` | 是 | 原始或归一化优先级 |
| `status` | 是 | 当前状态 |
| `createdAt` | 是 | 工单创建时间 |
| `groupAssignedAt` | 否 | 进入ITCC L2时间；无法准确获得时返回 `null` |
| `url` | 否 | Helix详情链接 |

插件会重新计算 `total` 和 `counts`，避免后端汇总字段与列表不一致。

## 错误响应

建议后端使用标准HTTP状态码，并返回：

```json
{
  "code": "HELIX_UNAVAILABLE",
  "message": "Helix暂时不可用"
}
```

插件不会直接展示后端堆栈或敏感信息。

