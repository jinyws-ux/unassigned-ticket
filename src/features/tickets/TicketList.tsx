import type { Ticket } from "../../types/ticket";
import { TicketCard } from "./TicketCard";

interface TicketListProps {
  tickets: Ticket[];
}

export function TicketList({ tickets }: TicketListProps) {
  return (
    <div className="ticket-list" aria-live="polite">
      {tickets.map((ticket) => (
        <TicketCard key={`${ticket.type}-${ticket.id}`} ticket={ticket} />
      ))}
    </div>
  );
}

