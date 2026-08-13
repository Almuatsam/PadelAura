import { z } from "zod"
import { api } from "@/lib/api"

const dailyOccupancySchema = z.object({ date: z.string(), bookingsCount: z.number() })
const dailyRevenueSchema = z.object({ date: z.string(), revenue: z.number() })
const busiestHourSchema = z.object({ hour: z.number(), count: z.number() })

const dashboardAnalyticsSchema = z.object({
  occupancy: z.array(dailyOccupancySchema),
  revenue: z.array(dailyRevenueSchema),
  busiestHours: z.array(busiestHourSchema),
  totalRevenue: z.number(),
  occupancyRatePercent: z.number(),
})

export type DashboardAnalytics = z.infer<typeof dashboardAnalyticsSchema>
export type AnalyticsRange = "week" | "month"

export async function fetchDashboardAnalytics(range: AnalyticsRange): Promise<DashboardAnalytics> {
  const response = await api.get("/admin/dashboard/analytics", { params: { range } })
  return dashboardAnalyticsSchema.parse(response.data)
}
