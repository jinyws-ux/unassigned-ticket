# ITCC 未分配工单 Outlook 插件

Windows 经典版 Outlook VSTO 插件。Outlook 启动后自动在每个主窗口右侧显示任务窗格，不依赖当前选中的邮件。

## 第一版能力

- Outlook 启动时自动显示右侧任务窗格
- 每个 Outlook Explorer 窗口独立创建任务窗格
- 每 60 秒自动查询，支持手动刷新
- 全部、INC、WO 筛选
- 数据库直连采用标准 ADO.NET Provider
- 数据库配置仅保存在当前 Windows 用户目录，并使用 DPAPI 加密
- 查询语句固定在代码中，插件界面不能提交任意 SQL
- 未配置数据库时显示设置引导，不会尝试连接

## 开发环境

- Windows 10/11
- 经典版 Outlook（VSTO 不支持新版 Outlook）
- Visual Studio 2022/2026
- Visual Studio 工作负载：`Office/SharePoint development`
- .NET Framework 4.8 Developer Pack

打开 `UnassignedTicket.OutlookAddIn.sln`，确认 Outlook 已安装后按 `F5` 调试。

## 数据库配置

第一次启动后，在右侧任务窗格点击“数据库设置”。配置内容会加密保存到：

```text
%LOCALAPPDATA%\ITCC\UnassignedTicket\database.config
```

建议优先使用 Windows 集成认证和只读数据库账号。不要把生产连接串提交到 Git。

当前查询位于 `Data/UnassignedTicketQuery.cs`。实际 SQL 需要把查询结果别名统一为：

| 别名 | 必填 | 说明 |
| --- | --- | --- |
| `TICKET_ID` | 是 | INC/WO 编号 |
| `TICKET_TYPE` | 否 | INC 或 WO；为空时从编号推断 |
| `SUMMARY` | 是 | 摘要 |
| `PRIORITY` | 否 | 优先级 |
| `STATUS` | 否 | 状态 |
| `CREATED_AT` | 是 | 创建时间 |
| `GROUP_ASSIGNED_AT` | 否 | 进入 ITCC L2 的时间 |
| `TICKET_URL` | 否 | Helix 跳转地址 |

## 数据库驱动

代码通过 `DbProviderFactories` 加载驱动：

- SQL Server 可使用 `.NET Framework Data Provider for SQL Server` 对应的 Provider 名称。
- Oracle 需要在项目中安装与你们环境匹配的 ODP.NET Provider，之后填写对应 Provider invariant name。

确定数据库类型、SQL 和返回字段后，再固定驱动包及真实字段映射。

## 安全边界

- 插件只执行内置只读查询。
- 不接受用户输入 SQL。
- 建议数据库账号只授予目标视图/表的 `SELECT` 权限。
- 连接串使用 Windows DPAPI `CurrentUser` 范围加密，每位用户需要单独配置。
