import type { ReactNode } from "react"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"

type EmptyStateProps = {
  icon?: ReactNode
  title: string
  description?: string
  action?: { label: string; onClick: () => void }
  /** "plain" for admin's calm variant — no organic blob shape. */
  shape?: "blob" | "plain"
  className?: string
}

export function EmptyState({ icon, title, description, action, shape = "blob", className }: EmptyStateProps) {
  return (
    <div
      className={cn(
        "flex flex-col items-center gap-3 border border-dashed border-border bg-card/60 px-6 py-10 text-center",
        shape === "blob" ? "rounded-blob-3" : "rounded-2xl",
        className,
      )}
    >
      {icon && <div className="text-muted-foreground/50 [&>svg]:size-12">{icon}</div>}
      <p className="font-display text-lg font-bold text-foreground">{title}</p>
      {description && <p className="max-w-sm text-sm text-muted-foreground">{description}</p>}
      {action && (
        <Button type="button" size="sm" onClick={action.onClick} className="mt-2">
          {action.label}
        </Button>
      )}
    </div>
  )
}
