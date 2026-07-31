import { useEffect, useState } from "react"
import type { FormEvent } from "react"
import { useTranslation } from "react-i18next"
import { Plus, Trash2 } from "lucide-react"

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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { Promotion, PromotionInput, PricingRule } from "@/features/admin/api/promotions"

function buildInitialRules(promotion: Promotion | undefined): PricingRule[] {
  return promotion?.rules ?? [{ minimumHours: 1, discountType: "FixedRate", discountValue: 0 }]
}

type Props = {
  open: boolean
  onOpenChange: (open: boolean) => void
  promotion?: Promotion
  onSubmit: (input: PromotionInput) => void
  isSubmitting: boolean
}

export function PromotionFormDialog({ open, onOpenChange, promotion, onSubmit, isSubmitting }: Props) {
  const { t } = useTranslation()
  const [name, setName] = useState("")
  const [isActive, setIsActive] = useState(true)
  const [rules, setRules] = useState<PricingRule[]>([])

  useEffect(() => {
    if (!open) return
    setName(promotion?.name ?? "")
    setIsActive(promotion?.isActive ?? true)
    setRules(buildInitialRules(promotion))
  }, [open, promotion])

  const updateRule = (index: number, patch: Partial<PricingRule>) => {
    setRules((current) => current.map((rule, i) => (i === index ? { ...rule, ...patch } : rule)))
  }

  const addRule = () => {
    setRules((current) => [...current, { minimumHours: 1, discountType: "FixedRate", discountValue: 0 }])
  }

  const removeRule = (index: number) => {
    setRules((current) => current.filter((_, i) => i !== index))
  }

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    onSubmit({ name, isActive, startDate: null, endDate: null, rules })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-xl">
        <DialogHeader>
          <DialogTitle>{promotion ? t("admin.promotions.edit") : t("admin.promotions.add")}</DialogTitle>
        </DialogHeader>

        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="promotion-name">{t("admin.promotions.name")}</Label>
            <Input id="promotion-name" value={name} onChange={(e) => setName(e.target.value)} required />
          </div>

          <div className="flex items-center gap-2.5">
            <Switch checked={isActive} onCheckedChange={setIsActive} />
            <Label>{t("admin.promotions.active")}</Label>
          </div>

          <div>
            <div className="mb-2 flex items-center justify-between">
              <Label>{t("admin.promotions.rules")}</Label>
              <Button type="button" variant="outline" size="sm" onClick={addRule}>
                <Plus className="size-3.5" />
                {t("admin.promotions.addRule")}
              </Button>
            </div>

            <div className="flex flex-col gap-2">
              {rules.map((rule, index) => (
                <div key={index} className="flex flex-wrap items-center gap-2 rounded-lg border border-border p-2.5">
                  <div className="flex flex-col gap-1">
                    <Label className="text-xs text-muted-foreground">{t("admin.promotions.minimumHours")}</Label>
                    <Input
                      type="number"
                      min="1"
                      className="w-20"
                      value={rule.minimumHours}
                      onChange={(e) => updateRule(index, { minimumHours: Number(e.target.value) })}
                    />
                  </div>

                  <div className="flex flex-col gap-1">
                    <Label className="text-xs text-muted-foreground">{t("admin.promotions.discountType")}</Label>
                    <Select
                      value={rule.discountType}
                      onValueChange={(value) => updateRule(index, { discountType: value as PricingRule["discountType"] })}
                    >
                      <SelectTrigger className="w-40"><SelectValue /></SelectTrigger>
                      <SelectContent>
                        <SelectItem value="FixedRate">{t("admin.promotions.fixedRate")}</SelectItem>
                        <SelectItem value="Percentage">{t("admin.promotions.percentage")}</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="flex flex-col gap-1">
                    <Label className="text-xs text-muted-foreground">{t("admin.promotions.discountValue")}</Label>
                    <Input
                      type="number"
                      min="0"
                      step="0.001"
                      className="w-24"
                      value={rule.discountValue}
                      onChange={(e) => updateRule(index, { discountValue: Number(e.target.value) })}
                    />
                  </div>

                  <Button
                    type="button"
                    variant="ghost"
                    size="icon-sm"
                    className="mt-4"
                    onClick={() => removeRule(index)}
                    disabled={rules.length === 1}
                  >
                    <Trash2 className="size-3.5 text-error" />
                  </Button>
                </div>
              ))}
            </div>
          </div>

          <DialogFooter>
            <Button type="submit" disabled={isSubmitting}>
              {t("admin.promotions.save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
