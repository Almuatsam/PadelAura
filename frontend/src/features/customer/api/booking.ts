import { z } from "zod"
import { api } from "@/lib/api"

export const bookingSlotSchema = z.object({
  date: z.string(),
  startTime: z.string(),
  endTime: z.string(),
})

const createBookingResultSchema = z.object({
  bookingReference: z.string(),
  total: z.number(),
  paymentUrl: z.string().nullable(),
})

export type CreateBookingResult = z.infer<typeof createBookingResultSchema>

export type CreateBookingInput = {
  phone: string
  fullName: string | null
  email: string | null
  paymentMethod: "PayOnArrival" | "Online"
  slots: { date: string; startTime: string; endTime: string }[]
}

export async function createBooking(input: CreateBookingInput): Promise<CreateBookingResult> {
  const response = await api.post("/customer/book", input)
  return createBookingResultSchema.parse(response.data)
}

const bookingStatusSchema = z.object({
  bookingReference: z.string(),
  status: z.enum(["Pending", "Confirmed", "Cancelled", "Completed"]),
  paymentMethod: z.enum(["PayOnArrival", "Online"]),
  paymentStatus: z.enum(["Unpaid", "Paid", "Failed", "Refunded"]),
  total: z.number(),
  slots: z.array(bookingSlotSchema),
})

export type BookingStatus = z.infer<typeof bookingStatusSchema>

export async function fetchBookingByReference(reference: string): Promise<BookingStatus> {
  const response = await api.get(`/customer/bookings/${reference}`)
  return bookingStatusSchema.parse(response.data)
}
