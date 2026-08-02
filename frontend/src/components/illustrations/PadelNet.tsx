import type { SVGProps } from "react"

export function PadelNet({ "aria-hidden": ariaHidden = true, ...props }: SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 60 30" fill="none" aria-hidden={ariaHidden} {...props}>
      <rect x="1.5" y="1.5" width="57" height="27" rx="3" stroke="currentColor" strokeWidth="2.5" />
      <path
        d="M11 1.5V28.5M21 1.5V28.5M31 1.5V28.5M41 1.5V28.5M51 1.5V28.5M1.5 10H58.5M1.5 20H58.5"
        stroke="currentColor"
        strokeWidth="1.25"
        opacity="0.55"
      />
    </svg>
  )
}
