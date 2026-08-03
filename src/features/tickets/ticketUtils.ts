import type { Ticket, TicketFilter } from "../../types/ticket";

export function getTicketReferenceTime(ticket: Ticket): string {
  return ticket.groupAssignedAt ?? ticket.createdAt;
}

export function getTicketAgeMinutes(ticket: Ticket, now = Date.now()): number {
  const timestamp = Date.parse(getTicketReferenceTime(ticket));
  if (Number.isNaN(timestamp)) return 0;
  return Math.max(0, Math.floor((now - timestamp) / 60_000));
}

export function sortTicketsByAge(tickets: Ticket[]): Ticket[] {
  return [...tickets].sort((left, right) => {
    return Date.parse(getTicketReferenceTime(left)) - Date.parse(getTicketReferenceTime(right));
  });
}

export function filterTickets(tickets: Ticket[], filter: TicketFilter): Ticket[] {
  return filter === "ALL" ? tickets : tickets.filter((ticket) => ticket.type === filter);
}

export function formatClockTime(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "--:--";
  return new Intl.DateTimeFormat("zh-CN", {
    hour: "2-digit",
    minute: "2-digit",
    hour12: false,
  }).format(date);
}

export function formatAge(minutes: number): string {
  if (minutes < 60) return `${minutes} 分钟`;
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;
  return remainingMinutes > 0 ? `${hours} 小时 ${remainingMinutes} 分钟` : `${hours} 小时`;
}

