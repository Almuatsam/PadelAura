import { z } from "zod"
import { api } from "@/lib/api"

export const pricingRuleSchema = z.object({
  minimumHours: z.number(),
  discountType: z.enum(["FixedRate", "Percentage"]),
  discountValue: z.number(),
})

export type PricingRule = z.infer<typeof pricingRuleSchema>

export const promotionSchema = z.object({
  id: z.number(),
  name: z.string(),
  isActive: z.boolean(),
  startDate: z.string().nullable(),
  endDate: z.string().nullable(),
  rules: z.array(pricingRuleSchema),
})

export type Promotion = z.infer<typeof promotionSchema>

export type PromotionInput = {
  name: string
  isActive: boolean
  startDate: string | null
  endDate: string | null
  rules: PricingRule[]
}

export async function fetchPromotions(): Promise<Promotion[]> {
  const response = await api.get("/admin/promotions")
  return z.array(promotionSchema).parse(response.data)
}

export async function createPromotion(input: PromotionInput): Promise<void> {
  await api.post("/admin/promotions", input)
}

export async function updatePromotion(id: number, input: PromotionInput): Promise<void> {
  await api.put(`/admin/promotions/${id}`, input)
}
