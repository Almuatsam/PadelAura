import { z } from "zod"
import { api } from "@/lib/api"

const dashboardSummarySchema = z.object({
  todayBookingsCount: z.number(),
  todayRevenue: z.number(),
  occupancyRate: z.number(),
})

export type DashboardSummary = z.infer<typeof dashboardSummarySchema>

export async function fetchDashboardSummary(): Promise<DashboardSummary> {
  const response = await api.get("/admin/dashboard")
  return dashboardSummarySchema.parse(response.data)
}
