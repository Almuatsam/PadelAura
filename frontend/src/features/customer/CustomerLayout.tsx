import { Link, Outlet } from "react-router-dom"
import { useTranslation } from "react-i18next"
import { Languages } from "lucide-react"

import { Button } from "@/components/ui/button"

export function CustomerLayout() {
  const { t, i18n } = useTranslation()

  const toggleLanguage = () => {
    void i18n.changeLanguage(i18n.language.startsWith("ar") ? "en" : "ar")
  }

  return (
    <div className="flex min-h-screen flex-col bg-background text-foreground">
      <header className="flex items-center justify-between border-b border-border bg-card px-4 py-3 md:px-8">
        <Link to="/" className="text-lg font-semibold text-primary">
          Padel Aura
        </Link>
        <Button variant="ghost" size="sm" onClick={toggleLanguage}>
          <Languages className="size-4" />
          {t("admin.common.language")}
        </Button>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  )
}
