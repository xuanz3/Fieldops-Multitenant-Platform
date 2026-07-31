import {
  BrowserRouter,
  Route,
  Routes,
} from 'react-router-dom'
import { AuthProvider } from './auth/AuthProvider'
import { AppShell } from './components/AppShell'
import { ProtectedRoute } from './components/ProtectedRoute'
import { ClientApprovalsPage } from './pages/ClientApprovalsPage'
import { CustomersPage } from './pages/CustomersPage'
import { DashboardPage } from './pages/DashboardPage'
import { DispatchPage } from './pages/DispatchPage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { TechnicianPage } from './pages/TechnicianPage'
import { WorkOrdersPage } from './pages/WorkOrdersPage'
import './App.css'
import './phase6.css'

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route
            path="/login"
            element={<LoginPage />}
          />

          <Route element={<ProtectedRoute />}>
            <Route element={<AppShell />}>
              <Route
                index
                element={<DashboardPage />}
              />
              <Route
                path="customers"
                element={<CustomersPage />}
              />
              <Route
                path="work-orders"
                element={<WorkOrdersPage />}
              />
              <Route
                path="dispatch"
                element={<DispatchPage />}
              />
              <Route
                path="technician"
                element={<TechnicianPage />}
              />
              <Route
                path="client-approvals"
                element={
                  <ClientApprovalsPage />
                }
              />
            </Route>
          </Route>

          <Route
            path="*"
            element={<NotFoundPage />}
          />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
