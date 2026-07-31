import { Link } from 'react-router'

export function NotFoundPage() {
  return (
    <main className="not-found">
      <p className="eyebrow">404</p>
      <h1>Page not found</h1>
      <p>
        The requested FieldOps page does not
        exist.
      </p>
      <Link
        className="button button-primary"
        to="/"
      >
        Return to dashboard
      </Link>
    </main>
  )
}
