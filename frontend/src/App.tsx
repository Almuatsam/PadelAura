import { lazy, Suspense } from "react"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"

import { AuthProvider, ProtectedRoute } from "@/lib/auth"
import { ToastProvider } from "@/lib/toast"

// Route-level code splitting: customers never download the admin bundle
// (which pulls in recharts) and admins never download the booking wizard.
const AdminLayout = lazy(() => import("@/features/admin/AdminLayout").then((m) => ({ default: m.AdminLayout })))
const LoginPage = lazy(() => import("@/features/admin/pages/LoginPage").then((m) => ({ default: m.LoginPage })))
const DashboardPage = lazy(() => import("@/features/admin/pages/DashboardPage").then((m) => ({ default: m.DashboardPage })))
const CourtsPage = lazy(() => import("@/features/admin/pages/CourtsPage").then((m) => ({ default: m.CourtsPage })))
const ClosuresPage = lazy(() => import("@/features/admin/pages/ClosuresPage").then((m) => ({ default: m.ClosuresPage })))
const BookingsPage = lazy(() => import("@/features/admin/pages/BookingsPage").then((m) => ({ default: m.BookingsPage })))
const PromotionsPage = lazy(() => import("@/features/admin/pages/PromotionsPage").then((m) => ({ default: m.PromotionsPage })))

const CustomerLayout = lazy(() => import("@/features/customer/CustomerLayout").then((m) => ({ default: m.CustomerLayout })))
const LandingPage = lazy(() => import("@/features/customer/pages/LandingPage").then((m) => ({ default: m.LandingPage })))
const BookingWizardPage = lazy(() => import("@/features/customer/pages/BookingWizardPage").then((m) => ({ default: m.BookingWizardPage })))
const BookingConfirmationPage = lazy(() =>
  import("@/features/customer/pages/BookingConfirmationPage").then((m) => ({ default: m.BookingConfirmationPage })),
)

const queryClient = new QueryClient()

function RouteFallback() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="h-8 w-8 animate-spin rounded-full border-2 border-current border-t-transparent" aria-label="Loading" role="status" />
    </div>
  )
}

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <AuthProvider>
          <BrowserRouter>
            <Suspense fallback={<RouteFallback />}>
              <Routes>
                <Route element={<CustomerLayout />}>
                  <Route path="/" element={<LandingPage />} />
                  <Route path="/book" element={<BookingWizardPage />} />
                  <Route path="/booking/:reference" element={<BookingConfirmationPage />} />
                </Route>
                <Route path="/admin/login" element={<LoginPage />} />
                <Route
                  path="/admin"
                  element={
                    <ProtectedRoute>
                      <AdminLayout />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<DashboardPage />} />
                  <Route path="analytics" element={<Navigate to="/admin" replace />} />
                  <Route path="courts" element={<CourtsPage />} />
                  <Route path="closures" element={<ClosuresPage />} />
                  <Route path="bookings" element={<BookingsPage />} />
                  <Route path="promotions" element={<PromotionsPage />} />
                </Route>
                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </Suspense>
          </BrowserRouter>
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>
  )
}

export default App
