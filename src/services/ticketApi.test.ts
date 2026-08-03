import { describe, expect, it } from "vitest";
import { normalizeTicketResponse } from "./ticketApi";

describe("normalizeTicketResponse", () => {
  it("keeps valid INC and WO tickets and recalculates counts", () => {
    const result = normalizeTicketResponse({
      group: "IT Control Center L2",
      generatedAt: "2026-08-03T12:00:00Z",
      tickets: [
        { id: "INC1", type: "INC", summary: "Incident", createdAt: "2026-08-03T11:00:00Z" },
        { id: "WO1", type: "WO", summary: "Work order", createdAt: "2026-08-03T11:30:00Z" },
        { id: "OTHER1", type: "OTHER" },
      ],
    });

    expect(result.total).toBe(2);
    expect(result.counts).toEqual({ INC: 1, WO: 1 });
    expect(result.tickets.map((ticket) => ticket.id)).toEqual(["INC1", "WO1"]);
  });

  it("rejects a response without a tickets array", () => {
    expect(() => normalizeTicketResponse({ group: "ITCC L2" })).toThrow("缺少 tickets 数组");
  });
});

