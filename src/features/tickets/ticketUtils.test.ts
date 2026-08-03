import { describe, expect, it } from "vitest";
import type { Ticket } from "../../types/ticket";
import { filterTickets, formatAge, getTicketAgeMinutes, sortTicketsByAge } from "./ticketUtils";

const tickets: Ticket[] = [
  { id: "INC2", type: "INC", summary: "newer", priority: "P3", status: "New", createdAt: "2026-08-03T11:40:00Z", groupAssignedAt: null, url: null },
  { id: "WO1", type: "WO", summary: "older", priority: "Low", status: "New", createdAt: "2026-08-03T10:00:00Z", groupAssignedAt: null, url: null },
];

describe("ticket utilities", () => {
  it("filters tickets by type", () => {
    expect(filterTickets(tickets, "INC").map((ticket) => ticket.id)).toEqual(["INC2"]);
  });

  it("sorts the oldest ticket first", () => {
    expect(sortTicketsByAge(tickets).map((ticket) => ticket.id)).toEqual(["WO1", "INC2"]);
  });

  it("uses group assigned time when it is available", () => {
    const ticket = { ...tickets[0], groupAssignedAt: "2026-08-03T11:50:00Z" };
    expect(getTicketAgeMinutes(ticket, Date.parse("2026-08-03T12:00:00Z"))).toBe(10);
  });

  it("formats durations", () => {
    expect(formatAge(20)).toBe("20 分钟");
    expect(formatAge(75)).toBe("1 小时 15 分钟");
  });
});

