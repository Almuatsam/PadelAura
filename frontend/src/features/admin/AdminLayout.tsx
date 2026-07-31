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

export function AdminLayout() {
  const { t, i18n } = useTranslation()
  const { logout } = useAuth()

  const toggleLanguage = () => {
    void i18n.changeLanguage(i18n.language.startsWith("ar") ? "en" : "ar")
  }

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      <aside className="hidden w-60 shrink-0 border-e border-border bg-sidebar p-4 md:flex md:flex-col">
        <div className="mb-6 px-2 text-lg font-semibold text-primary">Padel Aura</div>
        <nav className="flex flex-col gap-1">
          {navItems.map(({ to, key, icon: Icon, end }) => (
            <NavLink
              key={key}
              to={to}
              end={end}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                )
              }
            >
              <Icon className="size-4" />
              {t(`admin.nav.${key}`)}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center justify-between gap-2 border-b border-border bg-card px-4 py-3 md:justify-end md:px-6">
          <div className="text-lg font-semibold text-primary md:hidden">Padel Aura</div>
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

        <nav className="flex gap-1 overflow-x-auto border-b border-border bg-card px-4 py-2 md:hidden">
          {navItems.map(({ to, key, icon: Icon, end }) => (
            <NavLink
              key={key}
              to={to}
              end={end}
              className={({ isActive }) =>
                cn(
                  "flex shrink-0 items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium transition-colors",
                  isActive ? "bg-primary/10 text-primary" : "text-muted-foreground hover:bg-muted",
                )
              }
            >
              <Icon className="size-4" />
              {t(`admin.nav.${key}`)}
            </NavLink>
          ))}
        </nav>

        <main className="flex-1 p-4 md:p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
