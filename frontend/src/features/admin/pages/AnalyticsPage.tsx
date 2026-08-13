import { useState, type ReactNode } from "react"
import { useQuery } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
  type TooltipValueType,
} from "recharts"

import { Card, CardHeader, CardTitle, CardValue } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { fetchDashboardAnalytics, type AnalyticsRange } from "@/features/admin/api/analytics"

const ranges: AnalyticsRange[] = ["week", "month"]

const tooltipStyle = {
  background: "var(--popover)",
  border: "1px solid var(--border)",
  borderRadius: "0.75rem",
  color: "var(--popover-foreground)",
}

const axisTick = { fill: "var(--muted-foreground)", fontSize: 12 }

function formatShortDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString(undefined, { month: "short", day: "numeric" })
}

function formatDateLabel(label: ReactNode): string {
  return typeof label === "string" ? formatShortDate(label) : String(label ?? "")
}

function toNumber(value: TooltipValueType | undefined): number {
  return typeof value === "number" ? value : Number(value ?? 0)
}

function formatHour(hour: number): string {
  return `${String(hour).padStart(2, "0")}:00`
}

export function AnalyticsPage() {
  const { t } = useTranslation()
  const [range, setRange] = useState<AnalyticsRange>("week")

  const { data, isLoading } = useQuery({
    queryKey: ["admin-analytics", range],
    queryFn: () => fetchDashboardAnalytics(range),
  })

  const busiestHour = data?.busiestHours[0]
  const maxBusiestCount = busiestHour?.count ?? 0

  return (
    <div>
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <h1 className="font-display text-2xl font-bold">{t("admin.analytics.title")}</h1>
        <div className="flex gap-1 rounded-xl border border-border bg-card p-1">
          {ranges.map((option) => (
            <Button
              key={option}
              type="button"
              variant={range === option ? "default" : "ghost"}
              size="sm"
              onClick={() => setRange(option)}
            >
              {t(`admin.analytics.range.${option}`)}
            </Button>
          ))}
        </div>
      </div>

      <div className="mb-4 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card shape="plain" tint="orange">
          <CardHeader>
            <CardTitle>{t("admin.analytics.totalRevenue")}</CardTitle>
          </CardHeader>
          <CardValue>{isLoading ? "…" : `${data?.totalRevenue.toFixed(3)} OMR`}</CardValue>
        </Card>

        <Card shape="plain" tint="navy">
          <CardHeader>
            <CardTitle>{t("admin.analytics.occupancyRate")}</CardTitle>
          </CardHeader>
          <CardValue>{isLoading ? "…" : `${Math.round(data?.occupancyRatePercent ?? 0)}%`}</CardValue>
        </Card>

        <Card shape="plain" tint="amber">
          <CardHeader>
            <CardTitle>{t("admin.analytics.busiestHour")}</CardTitle>
          </CardHeader>
          <CardValue>{isLoading || !busiestHour ? "…" : formatHour(busiestHour.hour)}</CardValue>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <Card shape="plain" tint="flat">
          <CardHeader>
            <CardTitle>{t("admin.analytics.dailyRevenue")}</CardTitle>
          </CardHeader>
          {isLoading ? (
            <p className="text-sm text-muted-foreground">…</p>
          ) : (
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={data?.revenue} margin={{ left: -16 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                  <XAxis
                    dataKey="date"
                    tickFormatter={formatShortDate}
                    tick={axisTick}
                    axisLine={{ stroke: "var(--border)" }}
                    tickLine={false}
                  />
                  <YAxis tick={axisTick} axisLine={false} tickLine={false} width={48} />
                  <Tooltip
                    formatter={(value) => [`${toNumber(value).toFixed(3)} OMR`, t("admin.analytics.dailyRevenue")]}
                    labelFormatter={formatDateLabel}
                    contentStyle={tooltipStyle}
                  />
                  <Bar dataKey="revenue" fill="var(--chart-1)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}
        </Card>

        <Card shape="plain" tint="flat">
          <CardHeader>
            <CardTitle>{t("admin.analytics.dailyOccupancy")}</CardTitle>
          </CardHeader>
          {isLoading ? (
            <p className="text-sm text-muted-foreground">…</p>
          ) : (
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={data?.occupancy} margin={{ left: -16 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" vertical={false} />
                  <XAxis
                    dataKey="date"
                    tickFormatter={formatShortDate}
                    tick={axisTick}
                    axisLine={{ stroke: "var(--border)" }}
                    tickLine={false}
                  />
                  <YAxis allowDecimals={false} tick={axisTick} axisLine={false} tickLine={false} width={32} />
                  <Tooltip
                    formatter={(value) => [toNumber(value), t("admin.analytics.dailyOccupancy")]}
                    labelFormatter={formatDateLabel}
                    contentStyle={tooltipStyle}
                  />
                  <Line
                    type="monotone"
                    dataKey="bookingsCount"
                    stroke="var(--chart-4)"
                    strokeWidth={2}
                    dot={{ r: 3, fill: "var(--chart-4)" }}
                    activeDot={{ r: 5 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          )}
        </Card>
      </div>

      <Card shape="plain" tint="flat" className="mt-4">
        <CardHeader>
          <CardTitle>{t("admin.analytics.busiestHours")}</CardTitle>
        </CardHeader>
        {isLoading ? (
          <p className="text-sm text-muted-foreground">…</p>
        ) : data?.busiestHours.length === 0 ? (
          <p className="text-sm text-muted-foreground">{t("admin.analytics.noData")}</p>
        ) : (
          <ul className="flex flex-col gap-2">
            {data?.busiestHours.map((entry) => (
              <li key={entry.hour} className="flex items-center gap-3 text-sm">
                <span className="w-14 shrink-0 text-muted-foreground">{formatHour(entry.hour)}</span>
                <span className="h-2.5 flex-1 overflow-hidden rounded-full bg-muted">
                  <span
                    className="block h-full rounded-full bg-[var(--chart-2)]"
                    style={{ width: `${maxBusiestCount === 0 ? 0 : (entry.count / maxBusiestCount) * 100}%` }}
                  />
                </span>
                <span className="w-8 shrink-0 text-end font-semibold">{entry.count}</span>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </div>
  )
}
