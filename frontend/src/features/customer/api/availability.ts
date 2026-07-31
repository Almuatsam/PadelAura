import { z } from "zod"
import { api } from "@/lib/api"

export const availabilitySlotSchema = z.object({
  startTime: z.string(),
  endTime: z.string(),
  isAvailable: z.boolean(),
  price: z.number(),
})

export type AvailabilitySlot = z.infer<typeof availabilitySlotSchema>

export async function fetchAvailability(date: string): Promise<AvailabilitySlot[]> {
  const response = await api.get("/customer/availability", { params: { date } })
  return z.array(availabilitySlotSchema).parse(response.data)
}
