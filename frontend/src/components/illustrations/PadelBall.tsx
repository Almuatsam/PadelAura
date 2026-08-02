import type { SVGProps } from "react"

// Decorative padel-prop illustration, not a functional icon — same API shape as a
// lucide-react icon (currentColor-based, sizeable via className) so it drops in the same
// way, but defaults to aria-hidden since it conveys no information on its own.
export function PadelBall({ "aria-hidden": ariaHidden = true, ...props }: SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 40 40" fill="none" aria-hidden={ariaHidden} {...props}>
      <circle cx="20" cy="20" r="18" fill="currentColor" />
      <path
        d="M4 15 Q20 27 36 15"
        stroke="var(--card, #fff)"
        strokeWidth="2"
        fill="none"
        strokeLinecap="round"
      />
      <path
        d="M4 25 Q20 13 36 25"
        stroke="var(--card, #fff)"
        strokeWidth="2"
        fill="none"
        strokeLinecap="round"
      />
    </svg>
  )
}
