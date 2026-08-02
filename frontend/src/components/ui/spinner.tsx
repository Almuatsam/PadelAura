import { PadelBall } from "@/components/illustrations/PadelBall"
import { usePrefersReducedMotion } from "@/lib/usePrefersReducedMotion"
import { cn } from "@/lib/utils"

type SpinnerProps = {
  className?: string
  label?: string
}

export function Spinner({ className, label }: SpinnerProps) {
  const prefersReducedMotion = usePrefersReducedMotion()

  return (
    <span role="status" className={cn("inline-flex items-center gap-2", className)}>
      <PadelBall
        className={cn("size-5 text-primary", prefersReducedMotion ? "animate-pulse" : "animate-bounce-ball")}
      />
      <span className="sr-only">{label ?? "Loading"}</span>
    </span>
  )
}
