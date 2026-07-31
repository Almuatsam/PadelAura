import { useState } from "react"

import { slotKey, type CartSlot } from "@/features/customer/cart"
import { DateSlotStep } from "@/features/customer/pages/DateSlotStep"
import { ContactPaymentStep, type ContactInput } from "@/features/customer/pages/ContactPaymentStep"
import { ReviewStep } from "@/features/customer/pages/ReviewStep"

type Step = "slots" | "contact" | "review"

const emptyContact: ContactInput = { phone: "", fullName: "", email: "" }

export function BookingWizardPage() {
  const [step, setStep] = useState<Step>("slots")
  const [cart, setCart] = useState<CartSlot[]>([])
  const [contact, setContact] = useState<ContactInput>(emptyContact)
  const [paymentMethod, setPaymentMethod] = useState<"PayOnArrival" | "Online">("PayOnArrival")

  const addSlot = (slot: CartSlot) => setCart((current) => [...current, slot])

  const removeSlot = (date: string, startTime: string) =>
    setCart((current) => current.filter((slot) => slotKey(slot.date, slot.startTime) !== slotKey(date, startTime)))

  if (step === "contact") {
    return (
      <ContactPaymentStep
        defaultValues={contact}
        paymentMethod={paymentMethod}
        onPaymentMethodChange={setPaymentMethod}
        onBack={() => setStep("slots")}
        onNext={(values) => {
          setContact(values)
          setStep("review")
        }}
      />
    )
  }

  if (step === "review") {
    return (
      <ReviewStep
        cart={cart}
        contact={contact}
        paymentMethod={paymentMethod}
        onBack={() => setStep("contact")}
        onConflict={() => setStep("slots")}
      />
    )
  }

  return (
    <DateSlotStep
      cart={cart}
      onAddSlot={addSlot}
      onRemoveSlot={removeSlot}
      onContinue={() => setStep("contact")}
    />
  )
}
