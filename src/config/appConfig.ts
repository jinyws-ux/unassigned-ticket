interface RuntimeConfig {
  apiBaseUrl?: string;
  endpointPath?: string;
  refreshIntervalMs?: number;
  requestTimeoutMs?: number;
  useMockData?: boolean;
}

declare global {
  interface Window {
    __ITCC_CONFIG__?: RuntimeConfig;
  }
}

const runtime = window.__ITCC_CONFIG__ ?? {};

export const appConfig = {
  apiBaseUrl: (runtime.apiBaseUrl ?? "").replace(/\/$/, ""),
  endpointPath: runtime.endpointPath ?? "/api/v1/groups/itcc-l2/unassigned-tickets",
  refreshIntervalMs: runtime.refreshIntervalMs ?? 60_000,
  requestTimeoutMs: runtime.requestTimeoutMs ?? 15_000,
  useMockData: runtime.useMockData ?? true,
};

