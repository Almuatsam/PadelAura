import { useTranslation } from "react-i18next"
import { X } from "lucide-react"

import { Button } from "@/components/ui/button"
import { cartTotal, type CartSlot } from "@/features/customer/cart"
import { useBookingQuote } from "@/features/customer/useBookingQuote"

type Props = {
  cart: CartSlot[]
  onRemove?: (date: string, startTime: string) => void
}

export function CartSummary({ cart, onRemove }: Props) {
  const { t } = useTranslation()
  const { quote } = useBookingQuote(cart)

  if (cart.length === 0) {
    return <p className="text-sm text-muted-foreground">{t("customer.cart.empty")}</p>
  }

  const sorted = [...cart].sort((a, b) => (a.date + a.startTime).localeCompare(b.date + b.startTime))
  // The backend quote (same promotion logic used to charge the booking) is the source of truth;
  // the raw client-side sum is only a placeholder while it loads, never the final total shown.
  const total = quote?.total ?? cartTotal(cart)

  return (
    <div className="flex flex-col gap-2">
      <ul className="flex flex-col gap-1.5">
        {sorted.map((slot) => (
          <li
            key={`${slot.date}_${slot.startTime}`}
            className="flex items-center justify-between rounded-2xl border border-border bg-card px-3.5 py-2.5 text-sm"
          >
            <span>
              {slot.date} · {slot.startTime.slice(0, 5)}–{slot.endTime.slice(0, 5)}
            </span>
            <span className="flex items-center gap-2">
              <span className="font-medium">{slot.price.toFixed(3)} OMR</span>
              {onRemove && (
                <Button
                  type="button"
                  variant="ghost"
                  size="icon-sm"
                  aria-label={t("customer.cart.remove")}
                  onClick={() => onRemove(slot.date, slot.startTime)}
                >
                  <X className="size-3.5" />
                </Button>
              )}
            </span>
          </li>
        ))}
      </ul>
      {quote && quote.discount > 0 && (
        <>
          <div className="flex items-center justify-between text-sm text-muted-foreground">
            <span>{t("customer.cart.subtotal")}</span>
            <span>{quote.subtotal.toFixed(3)} OMR</span>
          </div>
          <div className="flex items-center justify-between text-sm text-success">
            <span>{t("customer.cart.discount")}</span>
            <span>-{quote.discount.toFixed(3)} OMR</span>
          </div>
        </>
      )}
      <div className="flex items-center justify-between border-t border-border pt-2 text-sm font-semibold">
        <span>{t("customer.cart.total")}</span>
        <span>{total.toFixed(3)} OMR</span>
      </div>
    </div>
  )
}
