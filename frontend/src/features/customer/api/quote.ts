import { z } from "zod"
import { api } from "@/lib/api"

const bookingQuoteSchema = z.object({
  subtotal: z.number(),
  discount: z.number(),
  total: z.number(),
})

export type BookingQuote = z.infer<typeof bookingQuoteSchema>

export type QuoteSlotInput = { date: string; startTime: string; endTime: string }

export async function fetchBookingQuote(slots: QuoteSlotInput[]): Promise<BookingQuote> {
  const response = await api.post("/customer/quote", { slots })
  return bookingQuoteSchema.parse(response.data)
}
