import { z } from "zod"
import { api } from "@/lib/api"

export const courtScheduleSchema = z.object({
  dayOfWeek: z.number().min(0).max(6),
  openTime: z.string(),
  closeTime: z.string(),
})

export type CourtSchedule = z.infer<typeof courtScheduleSchema>

export const courtSchema = z.object({
  id: z.number(),
  name: z.string(),
  hourPrice: z.number(),
  status: z.enum(["Active", "Inactive"]),
  schedules: z.array(courtScheduleSchema),
})

export type Court = z.infer<typeof courtSchema>

export type CourtInput = {
  name: string
  hourPrice: number
  status?: "Active" | "Inactive"
  schedules: CourtSchedule[]
}

export async function fetchCourts(): Promise<Court[]> {
  const response = await api.get("/admin/courts")
  return z.array(courtSchema).parse(response.data)
}

export async function createCourt(input: CourtInput): Promise<void> {
  await api.post("/admin/courts", input)
}

export async function updateCourt(id: number, input: CourtInput): Promise<void> {
  await api.put(`/admin/courts/${id}`, input)
}

export async function deleteCourt(id: number): Promise<void> {
  await api.delete(`/admin/courts/${id}`)
}
