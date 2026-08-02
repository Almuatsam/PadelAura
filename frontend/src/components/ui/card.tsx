import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"

import { cn } from "@/lib/utils"

// shape: blob-1..4 are the jelly-blob container shapes customer pages use (pick different
// ones for adjacent cards so a grid doesn't look stamped from one mold); "plain" is the
// admin calm-variant default (a standard rounded-2xl, no organic shape). tint: which
// candy-colored shadow the card casts — always match to the card's own dominant fill/use.
const cardVariants = cva(
  "flex flex-col gap-4 border border-border bg-card p-5 text-card-foreground transition-transform duration-200 hover:-translate-y-0.5",
  {
    variants: {
      shape: {
        "blob-1": "rounded-blob-1",
        "blob-2": "rounded-blob-2",
        "blob-3": "rounded-blob-3",
        "blob-4": "rounded-blob-4",
        plain: "rounded-2xl",
      },
      tint: {
        navy: "shadow-candy-navy",
        orange: "shadow-candy-orange",
        amber: "shadow-candy-amber",
        slate: "shadow-candy-slate",
        flat: "shadow-sm",
      },
    },
    defaultVariants: { shape: "blob-1", tint: "navy" },
  },
)

function Card({
  className,
  shape,
  tint,
  ...props
}: React.ComponentProps<"div"> & VariantProps<typeof cardVariants>) {
  return <div data-slot="card" className={cn(cardVariants({ shape, tint }), className)} {...props} />
}

function CardHeader({ className, ...props }: React.ComponentProps<"div">) {
  return <div data-slot="card-header" className={cn("flex flex-col gap-1", className)} {...props} />
}

function CardTitle({ className, ...props }: React.ComponentProps<"h3">) {
  return (
    <h3
      data-slot="card-title"
      className={cn("font-display text-sm font-semibold text-muted-foreground", className)}
      {...props}
    />
  )
}

function CardValue({ className, ...props }: React.ComponentProps<"p">) {
  return (
    <p
      data-slot="card-value"
      className={cn("font-display text-3xl font-bold tracking-tight", className)}
      {...props}
    />
  )
}

function CardContent({ className, ...props }: React.ComponentProps<"div">) {
  return <div data-slot="card-content" className={cn(className)} {...props} />
}

export { Card, CardHeader, CardTitle, CardValue, CardContent, cardVariants }
