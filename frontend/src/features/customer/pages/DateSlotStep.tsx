import { useState } from "react"
import { useQuery } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { cn } from "@/lib/utils"
import { fetchAvailability } from "@/features/customer/api/availability"
import { slotKey, type CartSlot } from "@/features/customer/cart"
import { CartSummary } from "@/features/customer/pages/CartSummary"

const todayIso = new Date().toISOString().slice(0, 10)

type Props = {
  cart: CartSlot[]
  onAddSlot: (slot: CartSlot) => void
  onRemoveSlot: (date: string, startTime: string) => void
  onContinue: () => void
}

export function DateSlotStep({ cart, onAddSlot, onRemoveSlot, onContinue }: Props) {
  const { t } = useTranslation()
  const [date, setDate] = useState(todayIso)

  const { data: slots, isLoading } = useQuery({
    queryKey: ["availability", date],
    queryFn: () => fetchAvailability(date),
    enabled: Boolean(date),
  })

  const selectedKeys = new Set(cart.map((slot) => slotKey(slot.date, slot.startTime)))

  const toggleSlot = (slot: { startTime: string; endTime: string; price: number }) => {
    const key = slotKey(date, slot.startTime)
    if (selectedKeys.has(key)) {
      onRemoveSlot(date, slot.startTime)
    } else {
      onAddSlot({ date, startTime: slot.startTime, endTime: slot.endTime, price: slot.price })
    }
  }

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-10">
      <div className="flex flex-col gap-1.5">
        <Label htmlFor="booking-date">{t("customer.slots.dateLabel")}</Label>
        <Input
          id="booking-date"
          type="date"
          min={todayIso}
          value={date}
          onChange={(e) => setDate(e.target.value)}
          className="w-48"
        />
      </div>

      <div>
        <p className="mb-2 text-sm text-muted-foreground">{t("customer.slots.legend")}</p>
        {isLoading && <p className="text-sm text-muted-foreground">{t("customer.slots.loading")}</p>}

        {!isLoading && slots?.length === 0 && (
          <p className="text-sm text-muted-foreground">{t("customer.slots.noneOpen")}</p>
        )}

        <div className="grid grid-cols-3 gap-2 sm:grid-cols-4 md:grid-cols-6">
          {slots?.map((slot) => {
            const key = slotKey(date, slot.startTime)
            const isSelected = selectedKeys.has(key)
            return (
              <button
                key={key}
                type="button"
                disabled={!slot.isAvailable}
                onClick={() => toggleSlot(slot)}
                className={cn(
                  "rounded-lg border px-2 py-2 text-sm font-medium transition-colors",
                  !slot.isAvailable && "cursor-not-allowed border-border bg-muted text-muted-foreground line-through",
                  slot.isAvailable && !isSelected && "border-border bg-card hover:border-primary hover:text-primary",
                  isSelected && "border-primary bg-primary text-primary-foreground",
                )}
              >
                {slot.startTime.slice(0, 5)}
              </button>
            )
          })}
        </div>
      </div>

      <div>
        <h2 className="mb-2 font-medium">{t("customer.cart.title")}</h2>
        <CartSummary cart={cart} onRemove={onRemoveSlot} />
      </div>

      <Button size="lg" disabled={cart.length === 0} onClick={onContinue}>
        {t("customer.slots.continue")}
      </Button>
    </div>
  )
}
