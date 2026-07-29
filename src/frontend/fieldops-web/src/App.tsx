import './App.css'

const capabilities = [
  'Multi-tenant boundaries',
  'Role-based work order workflows',
  'Auditable state changes',
  'Automated quality gates',
]

function App() {
  return (
    <main className="shell">
      <section className="hero" aria-labelledby="page-title">
        <p className="eyebrow">Enterprise full-stack portfolio project</p>
        <h1 id="page-title">FieldOps Hub</h1>
        <p className="summary">
          A multi-tenant operations platform for service requests, technician
          assignments, evidence, approvals and audit records.
        </p>
        <span className="status">Phase 1 · Foundation</span>
      </section>

      <section aria-labelledby="capabilities-title">
        <h2 id="capabilities-title">Engineering focus</h2>
        <ul className="capability-grid">
          {capabilities.map((capability) => (
            <li key={capability}>{capability}</li>
          ))}
        </ul>
      </section>
    </main>
  )
}

export default App
