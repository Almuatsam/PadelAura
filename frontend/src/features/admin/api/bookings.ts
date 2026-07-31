import { z } from "zod"
import { api } from "@/lib/api"

export const adminBookingItemSchema = z.object({
  courtName: z.string(),
  bookingDate: z.string(),
  startTime: z.string(),
  endTime: z.string(),
  price: z.number(),
})

export const adminBookingSchema = z.object({
  id: z.number(),
  bookingReference: z.string(),
  customerPhone: z.string(),
  customerName: z.string().nullable(),
  status: z.enum(["Pending", "Confirmed", "Cancelled", "Completed"]),
  paymentMethod: z.enum(["PayOnArrival", "Online"]),
  paymentStatus: z.enum(["Unpaid", "Paid", "Failed", "Refunded"]),
  total: z.number(),
  createdAt: z.string(),
  items: z.array(adminBookingItemSchema),
})

export type AdminBooking = z.infer<typeof adminBookingSchema>

export type BookingFilters = {
  courtId?: number
  date?: string
  status?: AdminBooking["status"]
  paymentMethod?: AdminBooking["paymentMethod"]
  phone?: string
  page: number
}

export async function fetchAdminBookings(filters: BookingFilters): Promise<AdminBooking[]> {
  const response = await api.get("/admin/bookings", { params: filters })
  return z.array(adminBookingSchema).parse(response.data)
}
