import { z } from "zod"
import { api } from "@/lib/api"

export const closureSchema = z.object({
  id: z.number(),
  courtId: z.number().nullable(),
  courtName: z.string().nullable(),
  closureDate: z.string(),
  startTime: z.string().nullable(),
  endTime: z.string().nullable(),
  reason: z.string().nullable(),
})

export type Closure = z.infer<typeof closureSchema>

export type CreateClosureInput = {
  courtIds: number[] | null
  date: string
  startTime: string | null
  endTime: string | null
  reason: string | null
}

export async function fetchClosures(): Promise<Closure[]> {
  const response = await api.get("/admin/closures")
  return z.array(closureSchema).parse(response.data)
}

export async function createClosure(input: CreateClosureInput): Promise<void> {
  await api.post("/admin/closures", input)
}
