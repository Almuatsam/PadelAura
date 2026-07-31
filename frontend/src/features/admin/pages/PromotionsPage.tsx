import { useState } from "react"
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { Plus, Pencil } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Card } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Switch } from "@/components/ui/switch"
import { useToast } from "@/lib/toast"
import {
  fetchPromotions,
  createPromotion,
  updatePromotion,
  type Promotion,
  type PromotionInput,
} from "@/features/admin/api/promotions"
import { PromotionFormDialog } from "@/features/admin/pages/PromotionFormDialog"

export function PromotionsPage() {
  const { t } = useTranslation()
  const { showToast } = useToast()
  const queryClient = useQueryClient()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingPromotion, setEditingPromotion] = useState<Promotion | undefined>(undefined)

  const { data: promotions, isLoading } = useQuery({ queryKey: ["promotions"], queryFn: fetchPromotions })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["promotions"] })

  const createMutation = useMutation({
    mutationFn: createPromotion,
    onSuccess: () => {
      invalidate()
      setDialogOpen(false)
      showToast(t("admin.promotions.save"))
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: number; input: PromotionInput }) => updatePromotion(id, input),
    onSuccess: () => {
      invalidate()
      setDialogOpen(false)
      showToast(t("admin.promotions.save"))
    },
  })

  const toggleActiveMutation = useMutation({
    mutationFn: ({ promotion, isActive }: { promotion: Promotion; isActive: boolean }) =>
      updatePromotion(promotion.id, {
        name: promotion.name,
        isActive,
        startDate: promotion.startDate,
        endDate: promotion.endDate,
        rules: promotion.rules,
      }),
    onSuccess: invalidate,
  })

  const openCreateDialog = () => {
    setEditingPromotion(undefined)
    setDialogOpen(true)
  }

  const openEditDialog = (promotion: Promotion) => {
    setEditingPromotion(promotion)
    setDialogOpen(true)
  }

  const handleSubmit = (input: PromotionInput) => {
    if (editingPromotion) {
      updateMutation.mutate({ id: editingPromotion.id, input })
    } else {
      createMutation.mutate(input)
    }
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold">{t("admin.promotions.title")}</h1>
        <Button onClick={openCreateDialog}>
          <Plus className="size-4" />
          {t("admin.promotions.add")}
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        {!isLoading &&
          promotions?.map((promotion) => (
            <Card key={promotion.id}>
              <div className="flex items-start justify-between">
                <div>
                  <h2 className="text-lg font-semibold">{promotion.name}</h2>
                  {promotion.rules.length === 0 ? (
                    <p className="text-sm text-muted-foreground">{t("admin.promotions.noRules")}</p>
                  ) : (
                    <ul className="mt-2 flex flex-col gap-1 text-sm text-muted-foreground">
                      {promotion.rules.map((rule, index) => (
                        <li key={index}>
                          {rule.minimumHours}h+ →{" "}
                          {rule.discountType === "FixedRate"
                            ? `${rule.discountValue.toFixed(3)} OMR/h`
                            : `${rule.discountValue}% off`}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant={promotion.isActive ? "success" : "muted"}>
                    {promotion.isActive ? t("admin.promotions.active") : t("admin.courts.inactive")}
                  </Badge>
                  <Button variant="ghost" size="icon-sm" onClick={() => openEditDialog(promotion)}>
                    <Pencil className="size-3.5" />
                  </Button>
                </div>
              </div>

              <div className="flex items-center gap-2.5 border-t border-border pt-3">
                <Switch
                  checked={promotion.isActive}
                  onCheckedChange={(checked) => toggleActiveMutation.mutate({ promotion, isActive: checked })}
                />
                <span className="text-sm text-muted-foreground">{t("admin.promotions.active")}</span>
              </div>
            </Card>
          ))}
      </div>

      <PromotionFormDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        promotion={editingPromotion}
        onSubmit={handleSubmit}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  )
}
