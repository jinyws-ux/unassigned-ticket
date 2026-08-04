namespace UnassignedTicket.OutlookAddIn.Data
{
    internal static class UnassignedTicketQuery
    {
        // 将下面查询替换成已确认的数据库 SQL。插件不会接受界面传入的任意 SQL。
        // WHERE 条件必须在数据库端限定 IT Control Center L2、ASCHG 为空和有效状态。
        internal const string Sql = @"
SELECT
    ticket_id         AS TICKET_ID,
    ticket_type       AS TICKET_TYPE,
    summary           AS SUMMARY,
    priority          AS PRIORITY,
    status            AS STATUS,
    created_at        AS CREATED_AT,
    group_assigned_at AS GROUP_ASSIGNED_AT,
    ticket_url        AS TICKET_URL
FROM your_ticket_view
WHERE support_group = 'IT Control Center L2'
  AND aschg IS NULL
  AND is_active = TRUE";

        internal static bool IsPlaceholder
        {
            get { return Sql.Contains("your_ticket_view"); }
        }
    }
}
