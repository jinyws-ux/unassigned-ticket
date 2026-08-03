# 架构说明

## 当前边界

第一版是只读监控插件，仅解决“发现ITCC L2组内无人处理的单子”。

```text
Outlook Task Pane
  -> ticketApi.ts
    -> 你的聚合后端
      -> Token缓存
      -> Helix INC API
      -> Helix WO API
```

## 前端模块

- `src/taskpane`：Outlook任务窗格入口和页面布局。
- `src/features/tickets`：工单列表、卡片、筛选和刷新逻辑。
- `src/services/ticketApi.ts`：唯一数据访问层。
- `src/config/appConfig.ts`：读取运行时配置。
- `src/mocks`：后端完成前使用的模拟数据。
- `src/components`：加载、空结果、错误状态。

## 安全要求

- Helix `apikey`、JWT、用户名和密码不得进入插件仓库或前端配置。
- 插件只调用内部后端，后端限制允许的来源和访问范围。
- 生产配置只能包含后端基础URL和非敏感UI配置。
- 后端返回给插件前应删除电话、邮箱、人员ID和完整Description等无关信息。

## 后续扩展顺序

1. Helix详情跳转。
2. 超时阈值配置。
3. Outlook通知。
4. “分配给我”（独立写操作模块，必须有身份校验、确认和审计）。
5. 已分配工单与统计视图。

新增功能应进入独立的 `features` 子目录，不把Helix业务字段写进UI组件。

