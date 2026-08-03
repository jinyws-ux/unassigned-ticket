import { useMemo, useState } from "react";
import { EmptyState } from "../components/EmptyState";
import { ErrorState } from "../components/ErrorState";
import { LoadingState } from "../components/LoadingState";
import { TicketList } from "../features/tickets/TicketList";
import { filterTickets, formatClockTime, sortTicketsByAge } from "../features/tickets/ticketUtils";
import { useTicketMonitor } from "../features/tickets/useTicketMonitor";
import type { TicketFilter } from "../types/ticket";

export function App() {
  const [filter, setFilter] = useState<TicketFilter>("ALL");
  const { data, error, isInitialLoading, isRefreshing, refresh } = useTicketMonitor();

  const visibleTickets = useMemo(() => {
    if (!data) return [];
    return sortTicketsByAge(filterTickets(data.tickets, filter));
  }, [data, filter]);

  const counts = {
    ALL: data?.total ?? 0,
    INC: data?.counts.INC ?? 0,
    WO: data?.counts.WO ?? 0,
  };

  return (
    <main className="app-shell">
      <header className="app-header">
        <div>
          <h1>未分配工单</h1>
          <div className="header-status">
            <span className={error ? "status-dot status-dot--warning" : "status-dot"} />
            <span>{data ? `${data.group} · 更新于 ${formatClockTime(data.generatedAt)}` : "ITCC L2"}</span>
          </div>
        </div>
        <button
          type="button"
          className="refresh-button"
          onClick={() => void refresh()}
          disabled={isRefreshing}
          aria-label="立即刷新工单"
          title="立即刷新"
        >
          <span className={isRefreshing ? "refresh-icon refresh-icon--spinning" : "refresh-icon"}>↻</span>
        </button>
      </header>

      {error && data && (
        <div className="stale-banner" role="status">
          <span>更新失败，当前显示 {formatClockTime(data.generatedAt)} 的结果</span>
          <button type="button" onClick={() => void refresh()}>重试</button>
        </div>
      )}

      <nav className="filter-tabs" aria-label="工单类型">
        {(["ALL", "INC", "WO"] as TicketFilter[]).map((item) => (
          <button
            type="button"
            key={item}
            className={filter === item ? "filter-tab filter-tab--active" : "filter-tab"}
            onClick={() => setFilter(item)}
          >
            <span>{item === "ALL" ? "全部" : item}</span>
            <strong>{counts[item]}</strong>
          </button>
        ))}
      </nav>

      <div className="list-header">
        <span>{visibleTickets.length} 个未分配工单</span>
        <span>等待最久优先</span>
      </div>

      <section className="content" aria-label="未分配工单列表">
        {isInitialLoading && <LoadingState />}
        {!isInitialLoading && error && !data && <ErrorState message={error} onRetry={() => void refresh()} />}
        {!isInitialLoading && data && visibleTickets.length === 0 && <EmptyState />}
        {!isInitialLoading && data && visibleTickets.length > 0 && <TicketList tickets={visibleTickets} />}
      </section>

      <footer className="app-footer">
        <span className="status-dot" />
        <span>每 60 秒自动刷新</span>
        <span className="footer-spacer" />
        <span>只读监控</span>
      </footer>
    </main>
  );
}

