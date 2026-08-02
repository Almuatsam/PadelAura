import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { Slot } from "radix-ui"
import { motion } from "framer-motion"

import { cn } from "@/lib/utils"
import { useBounceScale } from "@/lib/bounceScale"
import { usePrefersReducedMotion } from "@/lib/usePrefersReducedMotion"

// Glossy jelly-bean sheen: a background-image gradient layered over the variant's own
// background-color (a different CSS property, so it composes instead of overriding it).
// Reserved for the two solid brand-color fills; ghost/outline/link have no fill to shine.
const gloss =
  "relative overflow-hidden before:pointer-events-none before:absolute before:inset-0 before:rounded-[inherit] before:bg-gradient-to-b before:from-white/30 before:to-white/0"

const buttonVariants = cva(
  "group/button inline-flex shrink-0 items-center justify-center rounded-full border border-transparent bg-clip-padding text-sm font-bold whitespace-nowrap transition-all outline-none select-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:pointer-events-none disabled:opacity-50 aria-invalid:border-destructive aria-invalid:ring-3 aria-invalid:ring-destructive/20 dark:aria-invalid:border-destructive/50 dark:aria-invalid:ring-destructive/40 [&_svg]:pointer-events-none [&_svg]:shrink-0 [&_svg:not([class*='size-'])]:size-4",
  {
    variants: {
      variant: {
        default: cn("bg-primary text-primary-foreground shadow-candy-orange hover:bg-primary/90", gloss),
        outline:
          "border-border bg-background hover:bg-muted hover:text-foreground aria-expanded:bg-muted aria-expanded:text-foreground dark:border-input dark:bg-input/30 dark:hover:bg-input/50",
        secondary: cn("bg-secondary text-secondary-foreground shadow-candy-amber hover:bg-secondary/90", gloss),
        ghost: "hover:bg-muted hover:text-foreground aria-expanded:bg-muted aria-expanded:text-foreground dark:hover:bg-muted/50",
        destructive:
          "bg-destructive/10 text-destructive hover:bg-destructive/20 focus-visible:border-destructive/40 focus-visible:ring-destructive/20 dark:bg-destructive/20 dark:hover:bg-destructive/30 dark:focus-visible:ring-destructive/40",
        link: "text-primary underline-offset-4 hover:underline",
      },
      // Sizes below default/icon-sm are for dense, non-primary contexts (rarely used); default,
      // sm, icon and icon-sm are all real tap targets, so they meet or land close to 44px.
      // All sizes stay on rounded-full (the base) — a pill shape scales correctly with height,
      // so no per-size radius clamp is needed the way a fixed-px radius would require.
      size: {
        default: "h-11 gap-1.5 px-4 has-data-[icon=inline-end]:pe-3 has-data-[icon=inline-start]:ps-3",
        xs: "h-8 gap-1 px-2.5 text-xs has-data-[icon=inline-end]:pe-1.5 has-data-[icon=inline-start]:ps-1.5 [&_svg:not([class*='size-'])]:size-3",
        sm: "h-10 gap-1 px-3.5 text-[0.8rem] has-data-[icon=inline-end]:pe-2 has-data-[icon=inline-start]:ps-2 [&_svg:not([class*='size-'])]:size-3.5",
        lg: "h-12 gap-1.5 px-5 has-data-[icon=inline-end]:pe-3.5 has-data-[icon=inline-start]:ps-3.5",
        icon: "size-11",
        "icon-xs": "size-8 [&_svg:not([class*='size-'])]:size-3",
        "icon-sm": "size-10",
        "icon-lg": "size-12",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
    },
  }
)

// framer-motion redefines onDrag/onAnimationStart/onAnimationEnd with its own event
// signatures, which conflict with the native DOM handler types on <button> — omit them
// since this app doesn't use drag or the native animation-event props on Button.
type ButtonProps = Omit<
  React.ComponentProps<"button">,
  "onDrag" | "onDragStart" | "onDragEnd" | "onAnimationStart" | "onAnimationEnd"
> &
  VariantProps<typeof buttonVariants> & {
    asChild?: boolean
  }

function Button({
  className,
  variant = "default",
  size = "default",
  asChild = false,
  ...props
}: ButtonProps) {
  const Comp = asChild ? Slot.Root : "button"
  const MotionComp = motion.create(Comp)
  const bounceScale = useBounceScale()
  const prefersReducedMotion = usePrefersReducedMotion()

  const tapAnimation = prefersReducedMotion
    ? {}
    : {
        whileTap: { scale: 1 - 0.07 * bounceScale, scaleY: 1 + 0.06 * bounceScale },
        transition: { type: "spring" as const, stiffness: 500, damping: 22 },
      }

  return (
    <MotionComp
      data-slot="button"
      data-variant={variant}
      data-size={size}
      className={cn(buttonVariants({ variant, size, className }))}
      {...tapAnimation}
      {...props}
    />
  )
}

export { Button, buttonVariants }
