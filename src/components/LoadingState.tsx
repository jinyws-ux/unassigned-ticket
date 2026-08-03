export function LoadingState() {
  return (
    <div className="loading-state" aria-label="正在加载工单">
      {[0, 1, 2].map((item) => (
        <div className="skeleton-card" key={item}>
          <span className="skeleton skeleton--short" />
          <span className="skeleton skeleton--long" />
          <span className="skeleton skeleton--medium" />
        </div>
      ))}
    </div>
  );
}

