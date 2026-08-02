import { Link } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Zap, Grid2x2, ShieldCheck } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { HeroRacketStrike } from "@/components/illustrations/HeroRacketStrike"

const features = [
  { key: "fast", icon: Zap, shape: "blob-2", tint: "orange" },
  { key: "courts", icon: Grid2x2, shape: "blob-3", tint: "amber" },
  { key: "secure", icon: ShieldCheck, shape: "blob-4", tint: "navy" },
] as const

export function LandingPage() {
  const { t } = useTranslation()

  return (
    <div className="mx-auto flex max-w-5xl flex-col items-center gap-10 px-4 py-16 text-center md:py-24">
      <HeroRacketStrike className="w-52 cursor-pointer md:w-72" />

      <div className="flex flex-col gap-4">
        <h1 className="font-display text-4xl font-bold tracking-tight text-balance md:text-6xl">
          {t("customer.landing.title")}
        </h1>
        <p className="mx-auto max-w-xl text-muted-foreground md:text-lg">{t("customer.landing.subtitle")}</p>
      </div>

      <Button size="lg" asChild>
        <Link to="/book">{t("customer.landing.cta")}</Link>
      </Button>

      <div className="mt-6 grid w-full grid-cols-1 gap-5 sm:grid-cols-3">
        {features.map(({ key, icon: Icon, shape, tint }) => (
          <Card key={key} shape={shape} tint={tint} className="items-center gap-3 text-center">
            <span className="flex size-12 items-center justify-center rounded-full bg-primary/10 text-primary">
              <Icon className="size-6" />
            </span>
            <h2 className="font-display font-bold">{t(`customer.landing.features.${key}.title`)}</h2>
            <p className="text-sm text-muted-foreground">{t(`customer.landing.features.${key}.body`)}</p>
          </Card>
        ))}
      </div>
    </div>
  )
}
