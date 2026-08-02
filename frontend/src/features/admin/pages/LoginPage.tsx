import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useMutation } from "@tanstack/react-query"
import { useTranslation } from "react-i18next"
import { useNavigate } from "react-router-dom"
import { isAxiosError } from "axios"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useAuth } from "@/lib/auth"
import { BounceScaleContext } from "@/lib/bounceScale"
import { login, loginSchema, type LoginInput } from "@/features/admin/api/auth"

export function LoginPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { login: setAuthenticated } = useAuth()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({ resolver: zodResolver(loginSchema) })

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (token) => {
      setAuthenticated(token)
      navigate("/admin", { replace: true })
    },
  })

  const onSubmit = (input: LoginInput) => mutation.mutate(input)

  return (
    // Not nested under AdminLayout (this route sits outside ProtectedRoute), so the calm
    // admin scope is applied directly here rather than inherited.
    <BounceScaleContext.Provider value={0.4}>
      <div className="admin-scope flex min-h-screen items-center justify-center bg-background px-4">
        <div className="w-full max-w-sm rounded-2xl border border-border bg-card p-8 shadow-candy-navy">
          <h1 className="font-display text-xl font-bold text-primary">{t("admin.login.title")}</h1>
          <p className="mt-1 text-sm text-muted-foreground">{t("admin.login.subtitle")}</p>

        <form className="mt-6 flex flex-col gap-4" onSubmit={handleSubmit(onSubmit)}>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="email">{t("admin.login.email")}</Label>
            <Input id="email" type="email" autoComplete="username" {...register("email")} />
            {errors.email && <p className="text-xs text-error">{errors.email.message}</p>}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="password">{t("admin.login.password")}</Label>
            <Input id="password" type="password" autoComplete="current-password" {...register("password")} />
            {errors.password && <p className="text-xs text-error">{errors.password.message}</p>}
          </div>

          {mutation.isError && (
            <p className="text-sm text-error">
              {isAxiosError(mutation.error) && mutation.error.response?.status === 401
                ? t("admin.login.error")
                : (mutation.error as Error).message}
            </p>
          )}

          <Button type="submit" className="mt-2 w-full" disabled={mutation.isPending}>
            {t("admin.login.submit")}
          </Button>
        </form>
        </div>
      </div>
    </BounceScaleContext.Provider>
  )
}
