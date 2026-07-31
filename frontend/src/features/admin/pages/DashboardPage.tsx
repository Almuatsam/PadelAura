import { useQuery } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"

import { Card, CardHeader, CardTitle, CardValue } from "@/components/ui/card"
import { fetchDashboardSummary } from "@/features/admin/api/dashboard"

export function DashboardPage() {
  const { t } = useTranslation()

  const { data, isLoading } = useQuery({
    queryKey: ["dashboard-summary"],
    queryFn: fetchDashboardSummary,
  })

  return (
    <div>
      <h1 className="mb-6 text-2xl font-semibold">{t("admin.dashboard.title")}</h1>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>{t("admin.dashboard.todayBookings")}</CardTitle>
          </CardHeader>
          <CardValue>{isLoading ? "…" : data?.todayBookingsCount}</CardValue>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{t("admin.dashboard.todayRevenue")}</CardTitle>
          </CardHeader>
          <CardValue>{isLoading ? "…" : `${data?.todayRevenue.toFixed(3)} OMR`}</CardValue>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{t("admin.dashboard.occupancy")}</CardTitle>
          </CardHeader>
          <CardValue>{isLoading ? "…" : `${Math.round((data?.occupancyRate ?? 0) * 100)}%`}</CardValue>
        </Card>
      </div>
    </div>
  )
}
