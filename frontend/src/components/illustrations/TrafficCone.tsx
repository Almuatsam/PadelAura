import type { SVGProps } from "react"

export function TrafficCone({ "aria-hidden": ariaHidden = true, ...props }: SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 30 40" fill="none" aria-hidden={ariaHidden} {...props}>
      <polygon points="15,2 26,34 4,34" fill="currentColor" />
      <rect x="7" y="21" width="16" height="4" fill="var(--card, #fff)" opacity="0.85" />
      <rect x="2" y="34" width="26" height="5" rx="2" fill="currentColor" />
    </svg>
  )
}
