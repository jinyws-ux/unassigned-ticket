import type { Ticket, UnassignedTicketsResponse } from "../types/ticket";

function minutesAgo(minutes: number): string {
  return new Date(Date.now() - minutes * 60_000).toISOString();
}

export function createMockResponse(): UnassignedTicketsResponse {
  const tickets: Ticket[] = [
    {
      id: "INC00084721",
      type: "INC",
      summary: "生产数据库连接异常，客户端无法提交",
      priority: "P1",
      status: "Assigned",
      createdAt: minutesAgo(47),
      groupAssignedAt: minutesAgo(42),
      url: null,
    },
    {
      id: "INC00084735",
      type: "INC",
      summary: "Q站客户端登录后自动退出",
      priority: "P2",
      status: "Assigned",
      createdAt: minutesAgo(28),
      groupAssignedAt: null,
      url: null,
    },
    {
      id: "INC00084742",
      type: "INC",
      summary: "现场节点无法连接到应用服务",
      priority: "P3",
      status: "New",
      createdAt: minutesAgo(12),
      groupAssignedAt: minutesAgo(10),
      url: null,
    },
    {
      id: "WO00039216",
      type: "WO",
      summary: "新增车间操作员系统权限",
      priority: "Low",
      status: "Assigned",
      createdAt: minutesAgo(8),
      groupAssignedAt: null,
      url: null,
    },
  ];

  return {
    group: "IT Control Center L2",
    generatedAt: new Date().toISOString(),
    total: tickets.length,
    counts: {
      INC: tickets.filter((ticket) => ticket.type === "INC").length,
      WO: tickets.filter((ticket) => ticket.type === "WO").length,
    },
    tickets,
  };
}

