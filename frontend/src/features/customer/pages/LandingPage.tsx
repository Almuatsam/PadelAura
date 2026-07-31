import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Zap, Grid2x2, ShieldCheck } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import heroImg from "@/assets/hero.png"

const features = [
  { key: "fast", icon: Zap },
  { key: "courts", icon: Grid2x2 },
  { key: "secure", icon: ShieldCheck },
] as const

export function LandingPage() {
  const { t } = useTranslation()

  return (
    <div className="mx-auto flex max-w-5xl flex-col items-center gap-10 px-4 py-16 text-center md:py-24">
      <img src={heroImg} alt="" className="w-40 md:w-56" />

      <div className="flex flex-col gap-4">
        <h1 className="text-3xl font-semibold tracking-tight md:text-5xl">
          {t("customer.landing.title")}
        </h1>
        <p className="mx-auto max-w-xl text-muted-foreground md:text-lg">
          {t("customer.landing.subtitle")}
        </p>
      </div>

      <Button size="lg" asChild>
        <Link to="/book">{t("customer.landing.cta")}</Link>
      </Button>

      <div className="mt-6 grid w-full grid-cols-1 gap-4 sm:grid-cols-3">
        {features.map(({ key, icon: Icon }) => (
          <Card key={key} className="items-center text-center">
            <Icon className="size-6 text-primary" />
            <h2 className="font-medium">{t(`customer.landing.features.${key}.title`)}</h2>
            <p className="text-sm text-muted-foreground">{t(`customer.landing.features.${key}.body`)}</p>
          </Card>
        ))}
      </div>
    </div>
  )
}
