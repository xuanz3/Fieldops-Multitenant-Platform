import {
  chromium,
} from 'playwright'
import {
  mkdir,
  readFile,
  rm,
} from 'node:fs/promises'
import {
  dirname,
  join,
  resolve,
} from 'node:path'
import {
  fileURLToPath,
} from 'node:url'

const scriptDirectory =
  dirname(fileURLToPath(import.meta.url))
const repository =
  resolve(scriptDirectory, '../../../..')
const finalDirectory =
  join(
    repository,
    'docs/evidence/final',
  )
const videoDirectory =
  join(
    repository,
    'docs/evidence/video-runtime',
  )
const videoOutput =
  join(
    repository,
    'docs/evidence/fieldops-hub-demo.webm',
  )
const metadataPath =
  process.env.FIELDOPS_EVIDENCE_METADATA

if (!metadataPath) {
  throw new Error(
    'FIELDOPS_EVIDENCE_METADATA is required.',
  )
}

const metadata =
  JSON.parse(
    await readFile(
      metadataPath,
      'utf8',
    ),
  )

const baseUrl =
  process.env.FIELDOPS_BASE_URL

if (!baseUrl) {
  throw new Error(
    'FIELDOPS_BASE_URL is required.',
  )
}

await rm(
  finalDirectory,
  {
    recursive: true,
    force: true,
  },
)
await rm(
  videoDirectory,
  {
    recursive: true,
    force: true,
  },
)
await rm(
  videoOutput,
  {
    force: true,
  },
)
await mkdir(
  finalDirectory,
  { recursive: true },
)
await mkdir(
  videoDirectory,
  { recursive: true },
)

const browser =
  await chromium.launch({
    headless: true,
  })

const context =
  await browser.newContext({
    viewport: {
      width: 1440,
      height: 1000,
    },
    deviceScaleFactor: 1,
    recordVideo: {
      dir: videoDirectory,
      size: {
        width: 1440,
        height: 1000,
      },
    },
  })

const page =
  await context.newPage()
const video =
  page.video()

async function settle() {
  await page.waitForLoadState(
    'domcontentloaded',
  )

  await page.waitForTimeout(700)

  await page.addStyleTag({
    content: `
      *,
      *::before,
      *::after {
        animation-duration: 0s !important;
        transition-duration: 0s !important;
        caret-color: transparent !important;
      }

      html {
        scroll-behavior: auto !important;
      }
    `,
  })
}

async function capture(
  fileName,
) {
  await settle()

  await page.screenshot({
    path: join(
      finalDirectory,
      fileName,
    ),
    fullPage: true,
    animations: 'disabled',
  })

  await page.waitForTimeout(450)
}

async function login(
  roleButton,
) {
  await page.goto(
    `${baseUrl}/login`,
    {
      waitUntil:
        'domcontentloaded',
    },
  )

  await page
    .getByRole(
      'button',
      { name: roleButton },
    )
    .click()

  await page
    .getByRole(
      'button',
      {
        name:
          'Sign in to workspace',
      },
    )
    .click()

  await page.waitForURL(
    (url) =>
      !url.pathname.endsWith(
        '/login',
      ),
    {
      timeout: 20_000,
    },
  )

  await settle()
}

async function signOut() {
  const button =
    page.getByRole(
      'button',
      { name: 'Sign out' },
    )
    .first()

  if (
    await button.isVisible()
  ) {
    await button.click()
    await page.waitForURL(
      /\/login$/,
      {
        timeout: 15_000,
      },
    )
  }
}

async function goto(
  path,
) {
  await page.goto(
    `${baseUrl}${path}`,
    {
      waitUntil:
        'domcontentloaded',
    },
  )
  await settle()
}

await login('Dispatcher')

await goto('/')
await capture(
  '01-dashboard-overview.png',
)

await goto('/work-orders')
await capture(
  '02-work-orders-list.png',
)

await page
  .getByRole(
    'button',
    { name: 'Add work order' },
  )
  .click()
await page
  .getByRole('dialog')
  .waitFor()
await capture(
  '03-create-work-order.png',
)

await page
  .getByRole(
    'button',
    { name: 'Close' },
  )
  .click()

await page
  .getByRole(
    'button',
    { name: 'Edit' },
  )
  .first()
  .click()
await page
  .getByRole('dialog')
  .waitFor()
await capture(
  '04-work-order-detail.png',
)

await page
  .getByRole(
    'button',
    { name: 'Close' },
  )
  .click()

await goto('/dispatch')
await capture(
  '05-dispatch-board.png',
)

await goto('/evidence')
await capture(
  '07-completion-evidence.png',
)

await goto('/customers')
await capture(
  '09-customer-management.png',
)

await goto('/audit-log')
await capture(
  '11-audit-log.png',
)

await goto('/reports')
await capture(
  '12-reporting-dashboard.png',
)

await signOut()
await capture(
  '10-role-access-model.png',
)

await login('Technician')
await goto('/technician')
await capture(
  '06-technician-active-task.png',
)

await signOut()
await login('Client')
await goto('/client-approvals')
await capture(
  '08-client-approval.png',
)

await page.setContent(
  architectureHtml(),
  {
    waitUntil:
      'domcontentloaded',
  },
)
await capture(
  '13-architecture-tenant-isolation.png',
)

await page.setContent(
  ciHtml(metadata),
  {
    waitUntil:
      'domcontentloaded',
  },
)
await capture(
  '14-postgresql-isolation-ci.png',
)

const dashboard =
  await readFile(
    join(
      finalDirectory,
      '01-dashboard-overview.png',
    ),
  )

await page.setContent(
  deploymentHtml(
    metadata,
    dashboard.toString('base64'),
  ),
  {
    waitUntil:
      'domcontentloaded',
  },
)
await capture(
  '15-deployment-evidence.png',
)

await context.close()

if (video) {
  await video.saveAs(
    videoOutput,
  )
}

await rm(
  videoDirectory,
  {
    recursive: true,
    force: true,
  },
)

await browser.close()

console.log(
  `Final evidence written to ${finalDirectory}`,
)
console.log(
  `Demo video written to ${videoOutput}`,
)

function documentShell(
  title,
  body,
) {
  return `
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta
    name="viewport"
    content="width=device-width, initial-scale=1"
  >
  <title>${escapeHtml(title)}</title>
  <style>
    :root {
      color-scheme: light;
      font-family:
        Inter,
        ui-sans-serif,
        system-ui,
        -apple-system,
        BlinkMacSystemFont,
        "Segoe UI",
        sans-serif;
      background: #eef3f7;
      color: #16263a;
    }

    * {
      box-sizing: border-box;
    }

    body {
      min-height: 100vh;
      margin: 0;
      padding: 48px;
      background:
        radial-gradient(
          circle at top right,
          rgba(37, 111, 167, 0.16),
          transparent 32%
        ),
        #eef3f7;
    }

    .shell {
      max-width: 1340px;
      margin: 0 auto;
    }

    .eyebrow {
      margin: 0 0 10px;
      color: #39719b;
      font-size: 13px;
      font-weight: 800;
      letter-spacing: 0.12em;
      text-transform: uppercase;
    }

    h1 {
      margin: 0;
      font-size: 42px;
      line-height: 1.1;
    }

    .lead {
      max-width: 840px;
      margin: 16px 0 34px;
      color: #607287;
      font-size: 18px;
      line-height: 1.6;
    }

    .panel {
      padding: 28px;
      border: 1px solid #d8e1e9;
      border-radius: 18px;
      background: rgba(255, 255, 255, 0.96);
      box-shadow:
        0 22px 70px
        rgba(26, 52, 77, 0.10);
    }

    .grid {
      display: grid;
      grid-template-columns:
        repeat(3, minmax(0, 1fr));
      gap: 18px;
    }

    .card {
      padding: 20px;
      border: 1px solid #dce5ec;
      border-radius: 14px;
      background: #f8fbfd;
    }

    .card strong {
      display: block;
      margin-bottom: 8px;
      font-size: 17px;
    }

    .card span,
    .muted {
      color: #65778b;
      line-height: 1.55;
    }

    .flow {
      display: grid;
      grid-template-columns:
        1fr auto 1fr auto 1fr;
      gap: 14px;
      align-items: center;
      margin: 24px 0;
    }

    .node {
      min-height: 132px;
      display: grid;
      place-items: center;
      padding: 20px;
      border: 1px solid #cbd9e4;
      border-radius: 16px;
      background: white;
      text-align: center;
    }

    .node b {
      display: block;
      margin-bottom: 8px;
      color: #1c5278;
      font-size: 20px;
    }

    .arrow {
      color: #4e7797;
      font-size: 30px;
      font-weight: 700;
    }

    .tenant {
      margin-top: 22px;
      padding: 20px;
      border: 2px dashed #7ca1bd;
      border-radius: 16px;
      background: #eef6fb;
    }

    .tenant strong {
      color: #1b567e;
    }

    .status {
      display: inline-flex;
      align-items: center;
      gap: 9px;
      padding: 9px 13px;
      border-radius: 999px;
      background: #e7f7ee;
      color: #17643a;
      font-weight: 800;
    }

    .status::before {
      width: 9px;
      height: 9px;
      border-radius: 999px;
      background: #25a55b;
      content: "";
    }

    code {
      font-family:
        "SFMono-Regular",
        Consolas,
        monospace;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      margin-top: 20px;
    }

    th,
    td {
      padding: 15px 14px;
      border-bottom: 1px solid #e2e8ee;
      text-align: left;
    }

    th {
      color: #687a8d;
      font-size: 12px;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    .product {
      width: 100%;
      border: 1px solid #d7e0e7;
      border-radius: 14px;
      box-shadow:
        0 18px 50px
        rgba(30, 52, 74, 0.13);
    }

    .two-column {
      display: grid;
      grid-template-columns:
        0.92fr 1.35fr;
      gap: 24px;
      align-items: start;
    }

    .facts {
      display: grid;
      gap: 13px;
    }

    .fact {
      display: flex;
      justify-content: space-between;
      gap: 20px;
      padding: 14px 0;
      border-bottom: 1px solid #e1e8ee;
    }

    .fact:last-child {
      border-bottom: 0;
    }

    .fact span {
      color: #687b8f;
    }

    .fact strong {
      text-align: right;
    }
  </style>
</head>
<body>
  <main class="shell">
    ${body}
  </main>
</body>
</html>
`
}

function architectureHtml() {
  return documentShell(
    'FieldOps Hub architecture',
    `
      <p class="eyebrow">
        Final architecture
      </p>
      <h1>
        Tenant-safe field operations
      </h1>
      <p class="lead">
        A modular monolith keeps deployment
        simple while enforcing signed tenant
        identity, role ownership and database
        relationship boundaries.
      </p>

      <section class="panel">
        <div class="flow">
          <div class="node">
            <div>
              <b>React workspace</b>
              Role-aware routes,
              evidence and reports
            </div>
          </div>
          <div class="arrow">→</div>
          <div class="node">
            <div>
              <b>ASP.NET Core API</b>
              JWT policies,
              workflow and validation
            </div>
          </div>
          <div class="arrow">→</div>
          <div class="node">
            <div>
              <b>PostgreSQL</b>
              Query filters,
              composite tenant keys
              and append-only audit
            </div>
          </div>
        </div>

        <div class="tenant">
          <strong>
            Tenant boundary
          </strong>
          <p>
            Tenant identity is taken only from
            the validated token. Customer,
            WorkOrder, User, Attachment and
            AuditEvent records are constrained
            by TenantId at both API and database
            layers.
          </p>
        </div>

        <div class="grid">
          <article class="card">
            <strong>Authentication</strong>
            <span>
              PBKDF2 password hashes,
              signed JWT claims and
              role policies.
            </span>
          </article>
          <article class="card">
            <strong>Integrity</strong>
            <span>
              SHA-256 attachments,
              optimistic concurrency and
              audit hash chains.
            </span>
          </article>
          <article class="card">
            <strong>Deployment</strong>
            <span>
              Dockerized web, API and
              PostgreSQL services with
              health checks.
            </span>
          </article>
        </div>
      </section>
    `,
  )
}

function ciHtml(data) {
  const checks =
    data.ci.checks
      .map(
        (check) => `
          <tr>
            <td>${escapeHtml(check.name)}</td>
            <td>
              <span class="status">
                ${escapeHtml(check.conclusion)}
              </span>
            </td>
            <td>${escapeHtml(check.elapsed ?? '—')}</td>
          </tr>
        `,
      )
      .join('')

  return documentShell(
    'FieldOps Hub CI evidence',
    `
      <p class="eyebrow">
        GitHub Actions evidence
      </p>
      <h1>
        Exact-head validation passed
      </h1>
      <p class="lead">
        The Phase 8 pull request was validated
        on the exact commit shown below before
        final portfolio evidence was committed.
      </p>

      <section class="panel">
        <div class="two-column">
          <div class="facts">
            <div class="fact">
              <span>Repository</span>
              <strong>
                ${escapeHtml(data.repository)}
              </strong>
            </div>
            <div class="fact">
              <span>Pull request</span>
              <strong>
                #${escapeHtml(String(data.ci.pr))}
              </strong>
            </div>
            <div class="fact">
              <span>Workflow run</span>
              <strong>
                ${escapeHtml(String(data.ci.runId))}
              </strong>
            </div>
            <div class="fact">
              <span>Commit</span>
              <strong>
                <code>
                  ${escapeHtml(data.ci.commit.slice(0, 12))}
                </code>
              </strong>
            </div>
            <div class="fact">
              <span>Result</span>
              <strong>
                <span class="status">
                  ${escapeHtml(data.ci.conclusion)}
                </span>
              </strong>
            </div>
          </div>

          <div>
            <h2>
              Required checks
            </h2>
            <table>
              <thead>
                <tr>
                  <th>Check</th>
                  <th>Result</th>
                  <th>Elapsed</th>
                </tr>
              </thead>
              <tbody>
                ${checks}
              </tbody>
            </table>
          </div>
        </div>
      </section>
    `,
  )
}

function deploymentHtml(
  data,
  dashboardBase64,
) {
  const services =
    data.deployment.services
      .map(
        (service) => `
          <tr>
            <td>${escapeHtml(service.name)}</td>
            <td>
              <span class="status">
                ${escapeHtml(service.state)}
              </span>
            </td>
            <td>${escapeHtml(service.health)}</td>
          </tr>
        `,
      )
      .join('')

  return documentShell(
    'FieldOps Hub deployment evidence',
    `
      <p class="eyebrow">
        Production container deployment
      </p>
      <h1>
        Healthy three-service release
      </h1>
      <p class="lead">
        The final portfolio build runs through
        Nginx, ASP.NET Core and PostgreSQL with
        isolated networking, persistent data,
        migrations, demo seeding and health checks.
      </p>

      <section class="panel two-column">
        <div>
          <div class="facts">
            <div class="fact">
              <span>Deployment URL</span>
              <strong>
                ${escapeHtml(data.deployment.url)}
              </strong>
            </div>
            <div class="fact">
              <span>API health</span>
              <strong>
                <span class="status">
                  ${escapeHtml(data.deployment.apiHealth)}
                </span>
              </strong>
            </div>
            <div class="fact">
              <span>API phase</span>
              <strong>
                ${escapeHtml(String(data.deployment.apiPhase))}
              </strong>
            </div>
            <div class="fact">
              <span>Captured</span>
              <strong>
                ${escapeHtml(data.generatedAt)}
              </strong>
            </div>
          </div>

          <table>
            <thead>
              <tr>
                <th>Service</th>
                <th>State</th>
                <th>Health</th>
              </tr>
            </thead>
            <tbody>
              ${services}
            </tbody>
          </table>
        </div>

        <img
          class="product"
          alt="FieldOps Hub dashboard"
          src="data:image/png;base64,${dashboardBase64}"
        >
      </section>
    `,
  )
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;')
}
