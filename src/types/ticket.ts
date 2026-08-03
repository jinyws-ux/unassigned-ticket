export type TicketType = "INC" | "WO";

export interface Ticket {
  id: string;
  type: TicketType;
  summary: string;
  priority: string;
  status: string;
  createdAt: string;
  groupAssignedAt: string | null;
  url: string | null;
}

export interface TicketCounts {
  INC: number;
  WO: number;
}

export interface UnassignedTicketsResponse {
  group: string;
  generatedAt: string;
  total: number;
  counts: TicketCounts;
  tickets: Ticket[];
}

export type TicketFilter = "ALL" | TicketType;

