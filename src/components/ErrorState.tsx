interface ErrorStateProps {
  message: string;
  onRetry: () => void;
}

export function ErrorState({ message, onRetry }: ErrorStateProps) {
  return (
    <div className="error-state" role="alert">
      <strong>无法获取工单</strong>
      <p>{message}</p>
      <button type="button" onClick={onRetry}>重新加载</button>
    </div>
  );
}

