import { useEffect, useState } from "react"
import type { FormEvent } from "react"
import { useTranslation } from "react-i18next"

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import type { Court, CourtInput } from "@/features/admin/api/courts"

type ScheduleRow = {
  dayOfWeek: number
  isOpen: boolean
  openTime: string
  closeTime: string
}

const DEFAULT_OPEN = "08:00"
const DEFAULT_CLOSE = "23:00"

function toInputTime(value: string): string {
  return value.slice(0, 5)
}

function toApiTime(value: string): string {
  return value.length === 5 ? `${value}:00` : value
}

function buildInitialRows(court: Court | undefined): ScheduleRow[] {
  return Array.from({ length: 7 }, (_, dayOfWeek) => {
    const existing = court?.schedules.find((s) => s.dayOfWeek === dayOfWeek)
    return {
      dayOfWeek,
      isOpen: existing !== undefined,
      openTime: existing ? toInputTime(existing.openTime) : DEFAULT_OPEN,
      closeTime: existing ? toInputTime(existing.closeTime) : DEFAULT_CLOSE,
    }
  })
}

type Props = {
  open: boolean
  onOpenChange: (open: boolean) => void
  court?: Court
  onSubmit: (input: CourtInput) => void
  isSubmitting: boolean
}

export function CourtFormDialog({ open, onOpenChange, court, onSubmit, isSubmitting }: Props) {
  const { t } = useTranslation()
  const [name, setName] = useState("")
  const [hourPrice, setHourPrice] = useState("")
  const [status, setStatus] = useState<"Active" | "Inactive">("Active")
  const [rows, setRows] = useState<ScheduleRow[]>(() => buildInitialRows(undefined))

  useEffect(() => {
    if (!open) return
    setName(court?.name ?? "")
    setHourPrice(court ? String(court.hourPrice) : "")
    setStatus(court?.status ?? "Active")
    setRows(buildInitialRows(court))
  }, [open, court])

  const updateRow = (dayOfWeek: number, patch: Partial<ScheduleRow>) => {
    setRows((current) => current.map((row) => (row.dayOfWeek === dayOfWeek ? { ...row, ...patch } : row)))
  }

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    onSubmit({
      name,
      hourPrice: Number(hourPrice),
      status,
      schedules: rows
        .filter((row) => row.isOpen)
        .map((row) => ({
          dayOfWeek: row.dayOfWeek,
          openTime: toApiTime(row.openTime),
          closeTime: toApiTime(row.closeTime),
        })),
    })
  }

  const days = t("admin.courts.days", { returnObjects: true }) as string[]

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-xl">
        <DialogHeader>
          <DialogTitle>{court ? t("admin.courts.edit") : t("admin.courts.add")}</DialogTitle>
        </DialogHeader>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="court-name">{t("admin.courts.name")}</Label>
              <Input id="court-name" value={name} onChange={(e) => setName(e.target.value)} required />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="court-price">{t("admin.courts.hourPrice")}</Label>
              <Input
                id="court-price"
                type="number"
                step="0.001"
                min="0"
                value={hourPrice}
                onChange={(e) => setHourPrice(e.target.value)}
                required
              />
            </div>
          </div>

          {court && (
            <div className="flex items-center gap-2.5">
              <Switch
                id="court-status"
                checked={status === "Active"}
                onCheckedChange={(checked) => setStatus(checked ? "Active" : "Inactive")}
              />
              <Label htmlFor="court-status">
                {status === "Active" ? t("admin.courts.active") : t("admin.courts.inactive")}
              </Label>
            </div>
          )}

          <div>
            <Label className="mb-2">{t("admin.courts.schedule")}</Label>
            <div className="flex flex-col gap-2">
              {rows.map((row) => (
                <div key={row.dayOfWeek} className="flex items-center gap-3">
                  <Switch
                    checked={row.isOpen}
                    onCheckedChange={(checked) => updateRow(row.dayOfWeek, { isOpen: checked })}
                  />
                  <span className="w-24 shrink-0 text-sm">{days[row.dayOfWeek]}</span>
                  {row.isOpen ? (
                    <div className="flex flex-1 items-center gap-2">
                      <Input
                        type="time"
                        value={row.openTime}
                        onChange={(e) => updateRow(row.dayOfWeek, { openTime: e.target.value })}
                        className="w-32"
                      />
                      <span className="text-muted-foreground">–</span>
                      <Input
                        type="time"
                        value={row.closeTime}
                        onChange={(e) => updateRow(row.dayOfWeek, { closeTime: e.target.value })}
                        className="w-32"
                      />
                    </div>
                  ) : (
                    <span className="text-sm text-muted-foreground">{t("admin.courts.closed")}</span>
                  )}
                </div>
              ))}
            </div>
          </div>

          <DialogFooter>
            <Button type="submit" disabled={isSubmitting}>
              {t("admin.courts.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
