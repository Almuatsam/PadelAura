import { useEffect, useState } from "react"
import { NavLink, Outlet } from "react-router-dom"
import { useTranslation } from "react-i18next"
import {
  LayoutDashboard,
  Grid2x2,
  CalendarOff,
  ClipboardList,
  Tag,
  Languages,
  LogOut,
  Menu,
  X,
} from "lucide-react"

import { cn } from "@/lib/utils"
import { useAuth } from "@/lib/auth"
import { Button } from "@/components/ui/button"

const navItems = [
  { to: "/admin", key: "dashboard", icon: LayoutDashboard, end: true },
  { to: "/admin/courts", key: "courts", icon: Grid2x2, end: false },
  { to: "/admin/closures", key: "closures", icon: CalendarOff, end: false },
  { to: "/admin/bookings", key: "bookings", icon: ClipboardList, end: false },
  { to: "/admin/promotions", key: "promotions", icon: Tag, end: false },
] as const

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    "flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
    isActive
      ? "bg-primary/10 text-primary"
      : "text-muted-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
  )

export function AdminLayout() {
  const { t, i18n } = useTranslation()
  const { logout } = useAuth()
  const [menuOpen, setMenuOpen] = useState(false)

  const toggleLanguage = () => {
    void i18n.changeLanguage(i18n.language.startsWith("ar") ? "en" : "ar")
  }

  // Escape closes the mobile drawer
  useEffect(() => {
    if (!menuOpen) return
    const onKeyDown = (e: KeyboardEvent) => e.key === "Escape" && setMenuOpen(false)
    window.addEventListener("keydown", onKeyDown)
    return () => window.removeEventListener("keydown", onKeyDown)
  }, [menuOpen])

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      <aside className="hidden w-60 shrink-0 border-e border-border bg-sidebar p-4 md:flex md:flex-col">
        <div className="mb-6 px-2 text-lg font-semibold text-primary">Padel Aura</div>
        <nav className="flex flex-col gap-1">
          {navItems.map(({ to, key, icon: Icon, end }) => (
            <NavLink key={key} to={to} end={end} className={navLinkClass}>
              <Icon className="size-4" />
              {t(`admin.nav.${key}`)}
            </NavLink>
          ))}
        </nav>
      </aside>

      {/* Mobile drawer menu — overlay + slide-in panel from the inline-start edge */}
      {menuOpen && (
        <div className="fixed inset-0 z-50 md:hidden">
          <div className="absolute inset-0 bg-black/50" onClick={() => setMenuOpen(false)} />
          <nav className="absolute inset-y-0 start-0 flex w-64 max-w-[80%] flex-col gap-1 bg-sidebar p-4 shadow-lg">
            <div className="mb-4 flex items-center justify-between px-2">
              <span className="text-lg font-semibold text-primary">Padel Aura</span>
              <Button
                variant="ghost"
                size="icon-sm"
                aria-label={t("admin.nav.close")}
                onClick={() => setMenuOpen(false)}
              >
                <X className="size-4" />
              </Button>
            </div>
            {navItems.map(({ to, key, icon: Icon, end }) => (
              <NavLink key={key} to={to} end={end} className={navLinkClass} onClick={() => setMenuOpen(false)}>
                <Icon className="size-4" />
                {t(`admin.nav.${key}`)}
              </NavLink>
            ))}
          </nav>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex flex-wrap items-center justify-between gap-2 border-b border-border bg-card px-4 py-3 md:flex-nowrap md:justify-end md:px-6">
          <div className="flex items-center gap-2 md:hidden">
            <Button
              variant="ghost"
              size="icon"
              aria-label={t("admin.nav.menu")}
              onClick={() => setMenuOpen(true)}
            >
              <Menu className="size-5" />
            </Button>
            <span className="text-lg font-semibold text-primary">Padel Aura</span>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="ghost" size="sm" onClick={toggleLanguage}>
              <Languages className="size-4" />
              {t("admin.common.language")}
            </Button>
            <Button variant="ghost" size="sm" onClick={logout}>
              <LogOut className="size-4" />
              {t("admin.logout")}
            </Button>
          </div>
        </header>

        <main className="flex-1 p-4 md:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
