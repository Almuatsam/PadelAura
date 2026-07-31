import { useMutation, useQueryClient } from "@tanstack/react-query"
import { useNavigate } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { isAxiosError } from "axios"

import { Button } from "@/components/ui/button"
import { createBooking } from "@/features/customer/api/booking"
import { cartTotal, type CartSlot } from "@/features/customer/cart"
import { CartSummary } from "@/features/customer/pages/CartSummary"
import type { ContactInput } from "@/features/customer/pages/ContactPaymentStep"

type Props = {
  cart: CartSlot[]
  contact: ContactInput
  paymentMethod: "PayOnArrival" | "Online"
  onBack: () => void
  onConflict: () => void
}

export function ReviewStep({ cart, contact, paymentMethod, onBack, onConflict }: Props) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const mutation = useMutation({
    mutationFn: () =>
      createBooking({
        phone: contact.phone,
        fullName: contact.fullName || null,
        email: contact.email || null,
        paymentMethod,
        slots: cart.map(({ date, startTime, endTime }) => ({ date, startTime, endTime })),
      }),
    onSuccess: (result) => {
      if (paymentMethod === "Online" && result.paymentUrl) {
        window.location.href = result.paymentUrl
        return
      }
      navigate(`/booking/${result.bookingReference}`)
    },
    onError: (error) => {
      if (isAxiosError(error) && error.response?.status === 409) {
        void queryClient.invalidateQueries({ queryKey: ["availability"] })
      }
    },
  })

  const isConflict = isAxiosError(mutation.error) && mutation.error.response?.status === 409

  return (
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-10">
      <div>
        <h2 className="mb-2 font-medium">{t("customer.review.slots")}</h2>
        <CartSummary cart={cart} />
      </div>

      <div className="flex flex-col gap-1 text-sm">
        <span>
          <span className="text-muted-foreground">{t("customer.contact.phone")}: </span>
          {contact.phone}
        </span>
        {contact.fullName && (
          <span>
            <span className="text-muted-foreground">{t("customer.contact.fullName")}: </span>
            {contact.fullName}
          </span>
        )}
        <span>
          <span className="text-muted-foreground">{t("customer.contact.paymentMethod")}: </span>
          {paymentMethod === "PayOnArrival" ? t("customer.contact.payOnArrival") : t("customer.contact.payOnline")}
        </span>
      </div>

      <div className="flex items-center justify-between border-t border-border pt-3 text-lg font-semibold">
        <span>{t("customer.cart.total")}</span>
        <span>{cartTotal(cart).toFixed(3)} OMR</span>
      </div>

      {mutation.isError && (
        <p className="text-sm text-error">
          {isConflict ? t("customer.review.conflictError") : t("customer.review.genericError")}
        </p>
      )}

      <div className="flex gap-2">
        <Button type="button" variant="outline" onClick={isConflict ? onConflict : onBack} className="flex-1">
          {t("customer.back")}
        </Button>
        <Button
          type="button"
          className="flex-1"
          disabled={mutation.isPending}
          onClick={() => mutation.mutate()}
        >
          {t("customer.review.confirm")}
        </Button>
      </div>
    </div>
  )
}
