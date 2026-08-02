import { Link, NavLink, Outlet } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Languages, Home, CalendarPlus } from "lucide-react"

import { cn } from "@/lib/utils"
import { useLanguageToggle } from "@/lib/useLanguageToggle"
import { Button } from "@/components/ui/button"
import { PadelRacket } from "@/components/illustrations/PadelRacket"
import { PadelBall } from "@/components/illustrations/PadelBall"

const shelfNavItems = [
  { to: "/", key: "home", icon: Home, end: true },
  { to: "/book", key: "book", icon: CalendarPlus, end: false },
] as const

const shelfLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    "flex flex-col items-center gap-0.5 rounded-2xl px-4 py-1.5 text-xs font-bold transition-colors",
    isActive ? "text-primary" : "text-muted-foreground",
  )

export function CustomerLayout() {
  const { t } = useTranslation()
  const toggleLanguage = useLanguageToggle()

  return (
    <div className="relative flex min-h-screen flex-col bg-background text-foreground">
      {/* Corner props — ambient decoration, desktop only so they never compete with content
          on small screens where the toy-shelf nav already claims screen real estate. */}
      <PadelRacket
        className="pointer-events-none fixed -top-4 -end-4 hidden size-28 rotate-12 text-primary/10 lg:block"
      />
      <PadelBall
        className="pointer-events-none fixed bottom-10 -start-6 hidden size-20 text-secondary/20 lg:block"
      />

      <header className="relative z-10 flex items-center justify-between border-b border-border bg-card px-4 py-3 md:px-8">
        <Link to="/" className="font-display text-xl font-bold text-primary">
          Padel Aura
        </Link>
        <Button variant="ghost" size="sm" onClick={toggleLanguage}>
          <Languages className="size-4" />
          {t("admin.common.language")}
        </Button>
      </header>

      <main className="relative z-10 flex-1 pb-24 md:pb-0">
        <Outlet />
      </main>

      {/* Toy-shelf bottom nav — mobile only, icons sit like small objects on a shelf. */}
      <nav className="fixed inset-x-0 bottom-0 z-20 flex justify-center gap-2 border-t border-border bg-card px-4 pt-2 pb-[calc(0.5rem+env(safe-area-inset-bottom))] shadow-candy-navy md:hidden">
        {shelfNavItems.map(({ to, key, icon: Icon, end }) => (
          <NavLink key={key} to={to} end={end} className={shelfLinkClass}>
            <Icon className="size-5" />
            {t(`customer.nav.${key}`)}
          </NavLink>
        ))}
      </nav>
    </div>
  )
}
