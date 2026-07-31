interface PaginationProps {
  page: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
}

export function Pagination({
  page,
  totalPages,
  totalCount,
  onPageChange,
}: PaginationProps) {
  if (totalPages <= 1) {
    return (
      <div className="pagination-summary">
        {totalCount} record
        {totalCount === 1 ? '' : 's'}
      </div>
    )
  }

  return (
    <div className="pagination">
      <span>
        Page {page} of {totalPages} ·{' '}
        {totalCount} records
      </span>
      <div>
        <button
          className="button button-secondary"
          type="button"
          disabled={page <= 1}
          onClick={() =>
            onPageChange(page - 1)
          }
        >
          Previous
        </button>
        <button
          className="button button-secondary"
          type="button"
          disabled={page >= totalPages}
          onClick={() =>
            onPageChange(page + 1)
          }
        >
          Next
        </button>
      </div>
    </div>
  )
}
