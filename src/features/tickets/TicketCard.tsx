import type { Ticket } from "../../types/ticket";
import { formatAge, formatClockTime, getTicketAgeMinutes } from "./ticketUtils";

interface TicketCardProps {
  ticket: Ticket;
}

function priorityClass(priority: string): string {
  const normalized = priority.toLowerCase();
  if (normalized === "p1" || normalized === "critical") return "priority-critical";
  if (normalized === "p2" || normalized === "high") return "priority-high";
  return "priority-normal";
}

export function TicketCard({ ticket }: TicketCardProps) {
  const ageMinutes = getTicketAgeMinutes(ticket);
  const usesGroupTime = Boolean(ticket.groupAssignedAt);

  const openTicket = () => {
    if (!ticket.url) return;
    window.open(ticket.url, "_blank", "noopener,noreferrer");
  };

  return (
    <button
      type="button"
      className={`ticket-card ${ageMinutes >= 30 ? "ticket-overdue" : ""}`}
      onClick={openTicket}
      disabled={!ticket.url}
      aria-label={`${ticket.id}，${ticket.summary}`}
      title={ticket.url ? "在 Helix 中打开" : "演示数据暂无链接"}
    >
      <div className="ticket-card__topline">
        <div className="ticket-identity">
          <span className={`type-badge type-badge--${ticket.type.toLowerCase()}`}>{ticket.type}</span>
          <strong>{ticket.id}</strong>
        </div>
        <span className={`priority-badge ${priorityClass(ticket.priority)}`}>{ticket.priority}</span>
      </div>
      <p className="ticket-summary">{ticket.summary}</p>
      <div className="ticket-meta">
        <span className={ageMinutes >= 30 ? "age age--overdue" : "age"}>
          {usesGroupTime ? "未分配" : "创建"} {formatAge(ageMinutes)}
        </span>
        <span>{formatClockTime(ticket.createdAt)} 创建</span>
        {ticket.url && <span className="chevron" aria-hidden="true">›</span>}
      </div>
    </button>
  );
}

