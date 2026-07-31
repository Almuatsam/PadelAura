import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useTranslation } from "react-i18next"
import { z } from "zod"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group"

const contactSchema = z.object({
  phone: z.string().regex(/^\+?[0-9]{8,15}$/, "invalid"),
  fullName: z.string().optional(),
  email: z.string().email("invalid").optional().or(z.literal("")),
})

export type ContactInput = z.infer<typeof contactSchema>

type Props = {
  defaultValues: ContactInput
  paymentMethod: "PayOnArrival" | "Online"
  onPaymentMethodChange: (method: "PayOnArrival" | "Online") => void
  onBack: () => void
  onNext: (values: ContactInput) => void
}

export function ContactPaymentStep({ defaultValues, paymentMethod, onPaymentMethodChange, onBack, onNext }: Props) {
  const { t } = useTranslation()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ContactInput>({ resolver: zodResolver(contactSchema), defaultValues })

  return (
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-10">
      <form className="flex flex-col gap-4" onSubmit={handleSubmit(onNext)}>
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="phone">{t("customer.contact.phone")}</Label>
          <Input id="phone" type="tel" placeholder="+968XXXXXXXX" {...register("phone")} />
          {errors.phone && <p className="text-xs text-error">{t("customer.contact.phoneError")}</p>}
        </div>

        <div className="flex flex-col gap-1.5">
          <Label htmlFor="fullName">{t("customer.contact.fullName")}</Label>
          <Input id="fullName" {...register("fullName")} />
        </div>

        <div className="flex flex-col gap-1.5">
          <Label htmlFor="email">{t("customer.contact.email")}</Label>
          <Input id="email" type="email" {...register("email")} />
          {errors.email && <p className="text-xs text-error">{t("customer.contact.emailError")}</p>}
        </div>

        <div className="flex flex-col gap-2">
          <Label>{t("customer.contact.paymentMethod")}</Label>
          <RadioGroup
            value={paymentMethod}
            onValueChange={(value) => onPaymentMethodChange(value as "PayOnArrival" | "Online")}
          >
            <RadioGroupItem value="PayOnArrival">{t("customer.contact.payOnArrival")}</RadioGroupItem>
            <RadioGroupItem value="Online">{t("customer.contact.payOnline")}</RadioGroupItem>
          </RadioGroup>
        </div>

        <div className="mt-2 flex gap-2">
          <Button type="button" variant="outline" onClick={onBack} className="flex-1">
            {t("customer.back")}
          </Button>
          <Button type="submit" className="flex-1">
            {t("customer.contact.continue")}
          </Button>
        </div>
      </form>
    </div>
  )
}
