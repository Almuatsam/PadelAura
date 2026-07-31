import { useTranslation } from "react-i18next"
import { X } from "lucide-react"

import { Button } from "@/components/ui/button"
import { cartTotal, type CartSlot } from "@/features/customer/cart"

type Props = {
  cart: CartSlot[]
  onRemove?: (date: string, startTime: string) => void
}

export function CartSummary({ cart, onRemove }: Props) {
  const { t } = useTranslation()

  if (cart.length === 0) {
    return <p className="text-sm text-muted-foreground">{t("customer.cart.empty")}</p>
  }

  const sorted = [...cart].sort((a, b) => (a.date + a.startTime).localeCompare(b.date + b.startTime))

  return (
    <div className="flex flex-col gap-2">
      <ul className="flex flex-col gap-1.5">
        {sorted.map((slot) => (
          <li
            key={`${slot.date}_${slot.startTime}`}
            className="flex items-center justify-between rounded-lg border border-border bg-card px-3 py-2 text-sm"
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
                  size="icon-xs"
                  onClick={() => onRemove(slot.date, slot.startTime)}
                >
                  <X className="size-3" />
                </Button>
              )}
            </span>
          </li>
        ))}
      </ul>
      <div className="flex items-center justify-between border-t border-border pt-2 text-sm font-semibold">
        <span>{t("customer.cart.total")}</span>
        <span>{cartTotal(cart).toFixed(3)} OMR</span>
      </div>
    </div>
  )
}
