import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { Plus, Pencil, Trash2 } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Table, TableHeader, TableBody, TableRow, TableHead, TableCell } from "@/components/ui/table"
import { useToast } from "@/lib/toast"
import {
  fetchCourts,
  createCourt,
  updateCourt,
  deleteCourt,
  type Court,
  type CourtInput,
} from "@/features/admin/api/courts"
import { CourtFormDialog } from "@/features/admin/pages/CourtFormDialog"

export function CourtsPage() {
  const { t } = useTranslation()
  const { showToast } = useToast()
  const queryClient = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingCourt, setEditingCourt] = useState<Court | undefined>(undefined)

  const { data: courts, isLoading } = useQuery({ queryKey: ["courts"], queryFn: fetchCourts })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["courts"] })

  const createMutation = useMutation({
    mutationFn: createCourt,
    onSuccess: () => {
      invalidate()
      setDialogOpen(false)
      showToast(t("admin.courts.save"))
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: number; input: CourtInput }) => updateCourt(id, input),
    onSuccess: () => {
      invalidate()
      setDialogOpen(false)
      showToast(t("admin.courts.save"))
    },
  })

  const deleteMutation = useMutation({
    mutationFn: deleteCourt,
    onSuccess: () => {
      invalidate()
      showToast(t("admin.courts.delete"))
    },
  })

  const openCreateDialog = () => {
    setEditingCourt(undefined)
    setDialogOpen(true)
  }

  const openEditDialog = (court: Court) => {
    setEditingCourt(court)
    setDialogOpen(true)
  }

  const handleSubmit = (input: CourtInput) => {
    if (editingCourt) {
      updateMutation.mutate({ id: editingCourt.id, input })
    } else {
      createMutation.mutate(input)
    }
  }

  const handleDelete = (court: Court) => {
    if (window.confirm(t("admin.courts.confirmDelete"))) {
      deleteMutation.mutate(court.id)
    }
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="font-display text-2xl font-bold">{t("admin.courts.title")}</h1>
        <Button onClick={openCreateDialog}>
          <Plus className="size-4" />
          {t("admin.courts.add")}
        </Button>
      </div>

      <div className="rounded-2xl border border-border bg-card">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>{t("admin.courts.name")}</TableHead>
              <TableHead>{t("admin.courts.hourPrice")}</TableHead>
              <TableHead>{t("admin.courts.status")}</TableHead>
              <TableHead />
            </TableRow>
          </TableHeader>
          <TableBody>
            {!isLoading &&
              courts?.map((court) => (
                <TableRow key={court.id}>
                  <TableCell className="font-medium">{court.name}</TableCell>
                  <TableCell>{court.hourPrice.toFixed(3)} OMR</TableCell>
                  <TableCell>
                    <Badge variant={court.status === "Active" ? "success" : "muted"}>
                      {court.status === "Active" ? t("admin.courts.active") : t("admin.courts.inactive")}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        aria-label={t("admin.courts.edit")}
                        onClick={() => openEditDialog(court)}
                      >
                        <Pencil className="size-3.5" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon-sm"
                        aria-label={t("admin.courts.delete")}
                        onClick={() => handleDelete(court)}
                      >
                        <Trash2 className="size-3.5 text-error" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
          </TableBody>
        </Table>
      </div>

      <CourtFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        court={editingCourt}
        onSubmit={handleSubmit}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  )
}
