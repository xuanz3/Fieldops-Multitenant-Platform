interface FeedbackProps {
  title: string
  message: string
  tone?: 'neutral' | 'success' | 'error'
}

export function Feedback({
  title,
  message,
  tone = 'neutral',
}: FeedbackProps) {
  return (
    <div
      className={`feedback feedback-${tone}`}
      role={tone === 'error' ? 'alert' : 'status'}
    >
      <strong>{title}</strong>
      <span>{message}</span>
    </div>
  )
}
