import { Fragment, useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { ChevronDown, ChevronRight } from "lucide-react"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "@/components/ui/table"
import { fetchCourts } from "@/features/admin/api/courts"
import { fetchAdminBookings, type AdminBooking } from "@/features/admin/api/bookings"

const statusVariant: Record<AdminBooking["status"], "warning" | "success" | "error" | "muted"> = {
  Pending: "warning",
  Confirmed: "success",
  Cancelled: "error",
  Completed: "muted",
}

export function BookingsPage() {
  const { t } = useTranslation()
  const { data: courts } = useQuery({ queryKey: ["courts"], queryFn: fetchCourts })

  const [courtId, setCourtId] = useState("all")
  const [date, setDate] = useState("")
  const [status, setStatus] = useState("all")
  const [paymentMethod, setPaymentMethod] = useState("all")
  const [phone, setPhone] = useState("")
  const [page, setPage] = useState(1)
  const [expandedId, setExpandedId] = useState<number | null>(null)

  const { data: bookings, isLoading } = useQuery({
    queryKey: ["admin-bookings", courtId, date, status, paymentMethod, phone, page],
    queryFn: () =>
      fetchAdminBookings({
        courtId: courtId === "all" ? undefined : Number(courtId),
        date: date || undefined,
        status: status === "all" ? undefined : (status as AdminBooking["status"]),
        paymentMethod: paymentMethod === "all" ? undefined : (paymentMethod as AdminBooking["paymentMethod"]),
        phone: phone || undefined,
        page,
      }),
  })

  return (
    <div>
      <h1 className="mb-6 text-2xl font-semibold">{t("admin.bookings.title")}</h1>

      {/* Fluid on mobile (stacked, full-width); fixed once there's room from sm up */}
      <div className="mb-4 flex flex-wrap gap-3 rounded-2xl border border-border bg-card p-4">
        <Select value={courtId} onValueChange={(value) => { setCourtId(value); setPage(1) }}>
          <SelectTrigger className="w-full sm:w-40"><SelectValue placeholder={t("admin.bookings.filters.court")} /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("admin.bookings.filters.allCourts")}</SelectItem>
            {courts?.map((court) => (
              <SelectItem key={court.id} value={String(court.id)}>{court.name}</SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Input
          type="date"
          className="w-full sm:w-40"
          value={date}
          onChange={(e) => { setDate(e.target.value); setPage(1) }}
        />

        <Select value={status} onValueChange={(value) => { setStatus(value); setPage(1) }}>
          <SelectTrigger className="w-full sm:w-40"><SelectValue placeholder={t("admin.bookings.filters.status")} /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("admin.bookings.filters.allStatuses")}</SelectItem>
            <SelectItem value="Pending">Pending</SelectItem>
            <SelectItem value="Confirmed">Confirmed</SelectItem>
            <SelectItem value="Cancelled">Cancelled</SelectItem>
            <SelectItem value="Completed">Completed</SelectItem>
          </SelectContent>
        </Select>

        <Select value={paymentMethod} onValueChange={(value) => { setPaymentMethod(value); setPage(1) }}>
          <SelectTrigger className="w-full sm:w-40"><SelectValue placeholder={t("admin.bookings.filters.paymentMethod")} /></SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("admin.bookings.filters.allMethods")}</SelectItem>
            <SelectItem value="PayOnArrival">Pay on Arrival</SelectItem>
            <SelectItem value="Online">Online</SelectItem>
          </SelectContent>
        </Select>

        <Input
          className="w-full sm:w-40"
          placeholder={t("admin.bookings.filters.phone")}
          value={phone}
          onChange={(e) => { setPhone(e.target.value); setPage(1) }}
        />
      </div>

      {/* Table from sm up */}
      <div className="hidden rounded-2xl border border-border bg-card sm:block">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead />
              <TableHead>{t("admin.bookings.reference")}</TableHead>
              <TableHead>{t("admin.bookings.customer")}</TableHead>
              <TableHead>{t("admin.bookings.filters.status")}</TableHead>
              <TableHead>{t("admin.bookings.filters.paymentMethod")}</TableHead>
              <TableHead>{t("admin.bookings.total")}</TableHead>
              <TableHead>{t("admin.bookings.created")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {!isLoading && bookings?.length === 0 && (
              <TableRow>
                <TableCell colSpan={7} className="text-center text-muted-foreground">
                  {t("admin.bookings.noResults")}
                </TableCell>
              </TableRow>
            )}
            {!isLoading &&
              bookings?.map((booking) => (
                <Fragment key={booking.id}>
                  <TableRow
                    className="cursor-pointer"
                    onClick={() => setExpandedId(expandedId === booking.id ? null : booking.id)}
                  >
                    <TableCell>
                      {expandedId === booking.id ? <ChevronDown className="size-4" /> : <ChevronRight className="size-4" />}
                    </TableCell>
                    <TableCell className="font-medium">{booking.bookingReference}</TableCell>
                    <TableCell>{booking.customerName ?? booking.customerPhone}</TableCell>
                    <TableCell><Badge variant={statusVariant[booking.status]}>{booking.status}</Badge></TableCell>
                    <TableCell>{booking.paymentMethod}</TableCell>
                    <TableCell>{booking.total.toFixed(3)} OMR</TableCell>
                    <TableCell>{new Date(booking.createdAt).toLocaleString()}</TableCell>
                  </TableRow>
                  {expandedId === booking.id && (
                    <TableRow>
                      <TableCell colSpan={7} className="bg-muted/30">
                        <ul className="flex flex-col gap-1 text-sm">
                          {booking.items.map((item, index) => (
                            <li key={index}>
                              {item.courtName} — {item.bookingDate} {item.startTime.slice(0, 5)}–{item.endTime.slice(0, 5)} ({item.price.toFixed(3)} OMR)
                            </li>
                          ))}
                        </ul>
                      </TableCell>
                    </TableRow>
                  )}
                </Fragment>
              ))}
          </TableBody>
        </Table>
      </div>

      {/* Card list below sm — a 7-column table would only be readable via horizontal scroll */}
      <div className="flex flex-col gap-3 sm:hidden">
        {!isLoading && bookings?.length === 0 && (
          <p className="py-6 text-center text-sm text-muted-foreground">{t("admin.bookings.noResults")}</p>
        )}
        {!isLoading &&
          bookings?.map((booking) => (
            <Card
              key={booking.id}
              className="cursor-pointer gap-2 p-4"
              onClick={() => setExpandedId(expandedId === booking.id ? null : booking.id)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium">{booking.bookingReference}</span>
                <Badge variant={statusVariant[booking.status]}>{booking.status}</Badge>
              </div>
              <div className="flex items-center justify-between gap-2 text-sm text-muted-foreground">
                <span>{booking.customerName ?? booking.customerPhone}</span>
                <span>{booking.paymentMethod}</span>
              </div>
              <div className="flex items-center justify-between gap-2 border-t border-border pt-2 text-sm">
                <span className="font-semibold">{booking.total.toFixed(3)} OMR</span>
                <span className="text-muted-foreground">{new Date(booking.createdAt).toLocaleString()}</span>
              </div>
              {expandedId === booking.id && (
                <ul className="flex flex-col gap-1 border-t border-border pt-2 text-sm text-muted-foreground">
                  {booking.items.map((item, index) => (
                    <li key={index}>
                      {item.courtName} — {item.bookingDate} {item.startTime.slice(0, 5)}–{item.endTime.slice(0, 5)} ({item.price.toFixed(3)} OMR)
                    </li>
                  ))}
                </ul>
              )}
            </Card>
          ))}
      </div>

      <div className="mt-4 flex items-center justify-between">
        <Button variant="outline" size="sm" disabled={page === 1} onClick={() => setPage((p) => p - 1)}>
          {t("admin.bookings.previous")}
        </Button>
        <span className="text-sm text-muted-foreground">{t("admin.bookings.page", { page })}</span>
        <Button
          variant="outline"
          size="sm"
          disabled={(bookings?.length ?? 0) < 20}
          onClick={() => setPage((p) => p + 1)}
        >
          {t("admin.bookings.next")}
        </Button>
      </div>
    </div>
  )
}
