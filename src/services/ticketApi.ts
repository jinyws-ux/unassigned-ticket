import { appConfig } from "../config/appConfig";
import { createMockResponse } from "../mocks/tickets";
import type { Ticket, TicketType, UnassignedTicketsResponse } from "../types/ticket";

const validTicketTypes: TicketType[] = ["INC", "WO"];

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

function parseTicket(value: unknown): Ticket | null {
  if (!isRecord(value)) return null;
  if (typeof value.id !== "string" || !validTicketTypes.includes(value.type as TicketType)) {
    return null;
  }

  return {
    id: value.id,
    type: value.type as TicketType,
    summary: typeof value.summary === "string" ? value.summary : "无摘要",
    priority: typeof value.priority === "string" ? value.priority : "Unknown",
    status: typeof value.status === "string" ? value.status : "Unknown",
    createdAt: typeof value.createdAt === "string" ? value.createdAt : new Date().toISOString(),
    groupAssignedAt: typeof value.groupAssignedAt === "string" ? value.groupAssignedAt : null,
    url: typeof value.url === "string" && value.url.length > 0 ? value.url : null,
  };
}

export function normalizeTicketResponse(payload: unknown): UnassignedTicketsResponse {
  if (!isRecord(payload) || !Array.isArray(payload.tickets)) {
    throw new Error("接口返回格式不正确：缺少 tickets 数组");
  }

  const tickets = payload.tickets.map(parseTicket).filter((ticket): ticket is Ticket => ticket !== null);
  const counts = {
    INC: tickets.filter((ticket) => ticket.type === "INC").length,
    WO: tickets.filter((ticket) => ticket.type === "WO").length,
  };

  return {
    group: typeof payload.group === "string" ? payload.group : "IT Control Center L2",
    generatedAt: typeof payload.generatedAt === "string" ? payload.generatedAt : new Date().toISOString(),
    total: tickets.length,
    counts,
    tickets,
  };
}

function mergeAbortSignals(timeoutSignal: AbortSignal, externalSignal?: AbortSignal): AbortSignal {
  if (!externalSignal) return timeoutSignal;
  return AbortSignal.any([timeoutSignal, externalSignal]);
}

export async function fetchUnassignedTickets(signal?: AbortSignal): Promise<UnassignedTicketsResponse> {
  if (appConfig.useMockData) {
    await new Promise((resolve) => window.setTimeout(resolve, 250));
    return createMockResponse();
  }

  if (!appConfig.apiBaseUrl) {
    throw new Error("尚未配置后端 API 地址");
  }

  const timeoutSignal = AbortSignal.timeout(appConfig.requestTimeoutMs);
  const response = await fetch(`${appConfig.apiBaseUrl}${appConfig.endpointPath}`, {
    method: "GET",
    headers: { Accept: "application/json" },
    cache: "no-store",
    signal: mergeAbortSignals(timeoutSignal, signal),
  });

  if (!response.ok) {
    throw new Error(`获取工单失败（HTTP ${response.status}）`);
  }

  return normalizeTicketResponse(await response.json());
}

