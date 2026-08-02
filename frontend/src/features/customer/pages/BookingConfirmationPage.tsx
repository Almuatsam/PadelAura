import { useParams, useSearchParams } from "react-router-dom"
import { useQuery } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { RefreshCw } from "lucide-react"

import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Spinner } from "@/components/ui/spinner"
import { ConfettiBurst } from "@/components/ui/confetti-burst"
import { fetchBookingByReference } from "@/features/customer/api/booking"
import { statusVariant } from "@/lib/statusVariant"

export function BookingConfirmationPage() {
  const { t } = useTranslation()
  const { reference = "" } = useParams<{ reference: string }>()
  const [searchParams] = useSearchParams()
  const paymentStatusParam = searchParams.get("status")

  const { data: booking, isLoading, isError, refetch, isFetching } = useQuery({
    queryKey: ["booking", reference],
    queryFn: () => fetchBookingByReference(reference),
    enabled: Boolean(reference),
  })

  if (isLoading) {
    return <p className="px-4 py-10 text-center text-muted-foreground">{t("customer.confirmation.loading")}</p>
  }

  if (isError || !booking) {
    return <p className="px-4 py-10 text-center text-error">{t("customer.confirmation.notFound")}</p>
  }

  const isStillProcessing =
    booking.status === "Pending" && booking.paymentMethod === "Online" && paymentStatusParam !== "cancelled"

  return (
    <div className="relative mx-auto flex max-w-md flex-col gap-6 px-4 py-10">
      <ConfettiBurst trigger={booking.status === "Confirmed"} />

      <Card shape="blob-1" tint="orange" className="items-center gap-2 text-center">
        <p className="text-sm text-muted-foreground">{t("customer.confirmation.reference")}</p>
        <p className="font-display text-3xl font-bold tracking-wide">{booking.bookingReference}</p>
        <Badge variant={statusVariant[booking.status]}>
          {t(`customer.confirmation.status.${booking.status}`)}
        </Badge>
      </Card>

      {isStillProcessing && (
        <div className="flex items-center justify-between rounded-2xl border border-warning/20 bg-warning/10 p-3 text-sm text-warning">
          <span>{t("customer.confirmation.processing")}</span>
          {isFetching ? (
            <Spinner label={t("customer.confirmation.refresh")} />
          ) : (
            <Button
              variant="ghost"
              size="icon-sm"
              aria-label={t("customer.confirmation.refresh")}
              onClick={() => void refetch()}
            >
              <RefreshCw className="size-4" />
            </Button>
          )}
        </div>
      )}

      <div className="flex flex-col gap-1 text-sm">
        <span>
          <span className="text-muted-foreground">{t("customer.contact.paymentMethod")}: </span>
          {booking.paymentMethod === "PayOnArrival"
            ? t("customer.contact.payOnArrival")
            : t("customer.contact.payOnline")}
        </span>
        <span>
          <span className="text-muted-foreground">{t("customer.confirmation.paymentStatus")}: </span>
          {t(`customer.confirmation.paymentStatusValue.${booking.paymentStatus}`)}
        </span>
      </div>

      <div>
        <h2 className="font-display mb-2 font-bold">{t("customer.review.slots")}</h2>
        <ul className="flex flex-col gap-1.5">
          {booking.slots.map((slot) => (
            <li
              key={`${slot.date}_${slot.startTime}`}
              className="rounded-2xl border border-border bg-card px-3.5 py-2.5 text-sm"
            >
              {slot.date} · {slot.startTime.slice(0, 5)}–{slot.endTime.slice(0, 5)}
            </li>
          ))}
        </ul>
      </div>

      <div className="flex items-center justify-between border-t border-border pt-3 text-lg font-semibold">
        <span>{t("customer.cart.total")}</span>
        <span>{booking.total.toFixed(3)} OMR</span>
      </div>
    </div>
  )
}
