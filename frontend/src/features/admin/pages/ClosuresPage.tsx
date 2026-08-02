import { useState } from "react"
import type { FormEvent } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"

import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Checkbox } from "@/components/ui/checkbox"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "@/components/ui/table"
import { useToast } from "@/lib/toast"
import { fetchCourts } from "@/features/admin/api/courts"
import { fetchClosures, createClosure, type CreateClosureInput } from "@/features/admin/api/closures"

export function ClosuresPage() {
  const { t } = useTranslation()
  const { showToast } = useToast()
  const queryClient = useQueryClient()

  const { data: closures, isLoading } = useQuery({ queryKey: ["closures"], queryFn: fetchClosures })
  const { data: courts } = useQuery({ queryKey: ["courts"], queryFn: fetchCourts })

  const [allCourts, setAllCourts] = useState(true)
  const [selectedCourtIds, setSelectedCourtIds] = useState<number[]>([])
  const [date, setDate] = useState("")
  const [wholeDay, setWholeDay] = useState(true)
  const [startTime, setStartTime] = useState("08:00")
  const [endTime, setEndTime] = useState("23:00")
  const [reason, setReason] = useState("")

  const createMutation = useMutation({
    mutationFn: createClosure,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["closures"] })
      showToast(t("admin.closures.add"))
      setReason("")
    },
  })

  const toggleCourt = (courtId: number) => {
    setSelectedCourtIds((current) =>
      current.includes(courtId) ? current.filter((id) => id !== courtId) : [...current, courtId],
    )
  }

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const input: CreateClosureInput = {
      courtIds: allCourts ? null : selectedCourtIds,
      date,
      startTime: wholeDay ? null : `${startTime}:00`,
      endTime: wholeDay ? null : `${endTime}:00`,
      reason: reason || null,
    }
    createMutation.mutate(input)
  }

  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
      <Card shape="plain" className="lg:col-span-1">
        <h2 className="font-display text-lg font-bold">{t("admin.closures.add")}</h2>
        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <div className="flex items-center gap-2.5">
            <Switch checked={allCourts} onCheckedChange={setAllCourts} />
            <Label>{t("admin.closures.allCourts")}</Label>
          </div>

          {!allCourts && (
            <div className="flex flex-col gap-2 rounded-xl border border-border p-2.5">
              {courts?.map((court) => (
                <div key={court.id} className="flex items-center gap-2.5 text-sm">
                  <Checkbox
                    id={`closure-court-${court.id}`}
                    checked={selectedCourtIds.includes(court.id)}
                    onCheckedChange={() => toggleCourt(court.id)}
                  />
                  <Label htmlFor={`closure-court-${court.id}`} className="font-normal">
                    {court.name}
                  </Label>
                </div>
              ))}
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="closure-date">{t("admin.closures.date")}</Label>
            <Input id="closure-date" type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
          </div>

          <div className="flex items-center gap-2.5">
            <Switch checked={wholeDay} onCheckedChange={setWholeDay} />
            <Label>{t("admin.closures.wholeDay")}</Label>
          </div>

          {!wholeDay && (
            <div className="flex items-center gap-2">
              <Input type="time" value={startTime} onChange={(e) => setStartTime(e.target.value)} />
              <span className="text-muted-foreground">–</span>
              <Input type="time" value={endTime} onChange={(e) => setEndTime(e.target.value)} />
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="closure-reason">{t("admin.closures.reason")}</Label>
            <Input id="closure-reason" value={reason} onChange={(e) => setReason(e.target.value)} />
          </div>

          <Button type="submit" disabled={createMutation.isPending || !date}>
            {t("admin.closures.save")}
          </Button>
        </form>
      </Card>

      <Card shape="plain" className="lg:col-span-2">
        <h2 className="font-display text-lg font-bold">{t("admin.closures.upcoming")}</h2>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("admin.closures.court")}</TableHead>
              <TableHead>{t("admin.closures.date")}</TableHead>
              <TableHead>{t("admin.closures.startTime")}</TableHead>
              <TableHead>{t("admin.closures.reason")}</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {!isLoading &&
              closures?.map((closure) => (
                <TableRow key={closure.id}>
                  <TableCell>{closure.courtName ?? t("admin.closures.allCourts")}</TableCell>
                  <TableCell>{closure.closureDate}</TableCell>
                  <TableCell>
                    {closure.startTime && closure.endTime
                      ? `${closure.startTime.slice(0, 5)} – ${closure.endTime.slice(0, 5)}`
                      : t("admin.closures.wholeDay")}
                  </TableCell>
                  <TableCell>{closure.reason ?? "—"}</TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </Card>
    </div>
  )
}
