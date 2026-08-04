using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnassignedTicket.OutlookAddIn.Models;

namespace UnassignedTicket.OutlookAddIn.Data
{
    internal interface ITicketRepository
    {
        Task<IReadOnlyList<Ticket>> GetUnassignedTicketsAsync(CancellationToken cancellationToken);
        Task TestConnectionAsync(CancellationToken cancellationToken);
    }
}
