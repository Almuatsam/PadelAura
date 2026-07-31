import { z } from "zod"
import { api } from "@/lib/api"

export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1),
})

export type LoginInput = z.infer<typeof loginSchema>

const loginResponseSchema = z.object({
  token: z.string(),
  expiresAt: z.string(),
})

export async function login(input: LoginInput): Promise<string> {
  const response = await api.post("/auth/login", input)
  const parsed = loginResponseSchema.parse(response.data)
  return parsed.token
}
