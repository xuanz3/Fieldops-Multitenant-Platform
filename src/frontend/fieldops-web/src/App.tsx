import {
  BrowserRouter,
  Route,
  Routes,
} from 'react-router-dom'
import { AuthProvider } from './auth/AuthProvider'
import { AppShell } from './components/AppShell'
import { ProtectedRoute } from './components/ProtectedRoute'
import { CustomersPage } from './pages/CustomersPage'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { WorkOrdersPage } from './pages/WorkOrdersPage'
import './App.css'

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
