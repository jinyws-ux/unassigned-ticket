namespace UnassignedTicket.OutlookAddIn.Data
{
    internal static class UnassignedTicketQuery
    {
        // 查询 IT Control Center L2 中尚未分配处理人的有效 INC 和 WO。
        internal const string Sql = @"
SELECT
    id          AS TICKET_ID,
    ticket_type AS TICKET_TYPE,
    summary     AS SUMMARY,
    priority    AS PRIORITY,
    submit_time AS CREATED_AT
FROM remedy_ticket
WHERE assign_group = 'IT Control Center L2'
  AND status != 'Cancelled'
  AND assignee IS NULL";
    }
}
