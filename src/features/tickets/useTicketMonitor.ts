import { useCallback, useEffect, useRef, useState } from "react";
import { appConfig } from "../../config/appConfig";
import { fetchUnassignedTickets } from "../../services/ticketApi";
import type { UnassignedTicketsResponse } from "../../types/ticket";

interface TicketMonitorState {
  data: UnassignedTicketsResponse | null;
  error: string | null;
  isInitialLoading: boolean;
  isRefreshing: boolean;
  refresh: () => Promise<void>;
}

export function useTicketMonitor(): TicketMonitorState {
  const [data, setData] = useState<UnassignedTicketsResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const activeRequest = useRef<AbortController | null>(null);

  const refresh = useCallback(async () => {
    if (activeRequest.current) return;

    const controller = new AbortController();
    activeRequest.current = controller;
    setIsRefreshing(true);

    try {
      const response = await fetchUnassignedTickets(controller.signal);
      setData(response);
      setError(null);
    } catch (requestError) {
      if (!controller.signal.aborted) {
        setError(requestError instanceof Error ? requestError.message : "获取工单失败");
      }
    } finally {
      activeRequest.current = null;
      setIsRefreshing(false);
    }
  }, []);

  useEffect(() => {
    void refresh();
    const timer = window.setInterval(() => void refresh(), appConfig.refreshIntervalMs);

    return () => {
      window.clearInterval(timer);
      activeRequest.current?.abort();
    };
  }, [refresh]);

  return {
    data,
    error,
    isInitialLoading: !data && isRefreshing,
    isRefreshing,
    refresh,
  };
}

