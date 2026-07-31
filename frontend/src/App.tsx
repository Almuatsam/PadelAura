import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom"

import { AuthProvider, ProtectedRoute } from "@/lib/auth"
import { ToastProvider } from "@/lib/toast"
import { AdminLayout } from "@/features/admin/AdminLayout"
import { LoginPage } from "@/features/admin/pages/LoginPage"
import { DashboardPage } from "@/features/admin/pages/DashboardPage"
import { CourtsPage } from "@/features/admin/pages/CourtsPage"
import { ClosuresPage } from "@/features/admin/pages/ClosuresPage"
import { BookingsPage } from "@/features/admin/pages/BookingsPage"
import { PromotionsPage } from "@/features/admin/pages/PromotionsPage"
import { CustomerLayout } from "@/features/customer/CustomerLayout"
import { LandingPage } from "@/features/customer/pages/LandingPage"
import { BookingWizardPage } from "@/features/customer/pages/BookingWizardPage"
import { BookingConfirmationPage } from "@/features/customer/pages/BookingConfirmationPage"

const queryClient = new QueryClient()

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <AuthProvider>
          <BrowserRouter>
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
                <Route path="courts" element={<CourtsPage />} />
                <Route path="closures" element={<ClosuresPage />} />
                <Route path="bookings" element={<BookingsPage />} />
                <Route path="promotions" element={<PromotionsPage />} />
              </Route>
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </BrowserRouter>
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>
  )
}

export default App
