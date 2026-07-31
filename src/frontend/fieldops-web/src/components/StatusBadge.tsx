import type {
  WorkOrderPriority,
  WorkOrderStatus,
} from '../types'

type BadgeValue =
  | WorkOrderPriority
  | WorkOrderStatus
  | 'Active'
  | 'Restricted'

interface StatusBadgeProps {
  value: BadgeValue
}

function className(value: BadgeValue) {
  return `status-badge status-${value
    .replace(/([a-z])([A-Z])/g, '$1-$2')
    .toLowerCase()}`
}

export function StatusBadge({
  value,
}: StatusBadgeProps) {
  return (
    <span className={className(value)}>
      {value.replace(
        /([a-z])([A-Z])/g,
        '$1 $2',
      )}
    </span>
  )
}
