export function EmptyState() {
  return (
    <div className="empty-state">
      <span className="empty-state__icon" aria-hidden="true">✓</span>
      <strong>当前没有未分配工单</strong>
      <p>ITCC L2 组内工单均已分配</p>
    </div>
  );
}

