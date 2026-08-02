import type { SVGProps } from "react"

export function PadelRacket({ "aria-hidden": ariaHidden = true, ...props }: SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 40 60" fill="none" aria-hidden={ariaHidden} {...props}>
      <rect x="6" y="2" width="28" height="34" rx="14" fill="currentColor" fillOpacity="0.16" stroke="currentColor" strokeWidth="2.5" />
      <circle cx="14" cy="14" r="1.6" fill="currentColor" />
      <circle cx="20" cy="10" r="1.6" fill="currentColor" />
      <circle cx="26" cy="14" r="1.6" fill="currentColor" />
      <circle cx="14" cy="24" r="1.6" fill="currentColor" />
      <circle cx="20" cy="28" r="1.6" fill="currentColor" />
      <circle cx="26" cy="24" r="1.6" fill="currentColor" />
      <rect x="16" y="35" width="8" height="21" rx="3" fill="currentColor" />
    </svg>
  )
}
