namespace UnassignedTicket.OutlookAddIn.Data
{
    internal static class UnassignedTicketQuery
    {
        // 将下面查询替换成已确认的数据库 SQL。插件不会接受界面传入的任意 SQL。
        // WHERE 条件必须在数据库端限定 IT Control Center L2、ASCHG 为空和有效状态。
        internal const string Sql = @"
SELECT
    TICKET_ID,
    TICKET_TYPE,
    SUMMARY,
    PRIORITY,
    STATUS,
    CREATED_AT,
    GROUP_ASSIGNED_AT,
    TICKET_URL
FROM YOUR_TICKET_VIEW
WHERE SUPPORT_GROUP = 'IT Control Center L2'
  AND ASCHG IS NULL
  AND IS_ACTIVE = 1";

        internal static bool IsPlaceholder
        {
            get { return Sql.Contains("YOUR_TICKET_VIEW"); }
        }
    }
}
