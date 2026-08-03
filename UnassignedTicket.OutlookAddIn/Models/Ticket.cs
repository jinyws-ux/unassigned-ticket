using System;

namespace UnassignedTicket.OutlookAddIn.Models
{
    internal sealed class Ticket
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Summary { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? GroupAssignedAt { get; set; }
        public string Url { get; set; }

        public TimeSpan WaitingTime
        {
            get { return DateTime.Now - (GroupAssignedAt ?? CreatedAt); }
        }
    }
}
