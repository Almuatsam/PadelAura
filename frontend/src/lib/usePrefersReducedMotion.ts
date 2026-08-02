import { useEffect, useState } from "react"

const QUERY = "(prefers-reduced-motion: reduce)"

function getInitial(): boolean {
  if (typeof window === "undefined") return false
  return window.matchMedia(QUERY).matches
}

/**
 * framer-motion animations run via JS/WAAPI, not CSS transitions, so they don't
 * automatically respect the global `prefers-reduced-motion` CSS block in index.css.
 * Components using framer-motion (tap-spring, Spinner, ConfettiBurst) check this
 * explicitly instead.
 */
export function usePrefersReducedMotion(): boolean {
  const [reduced, setReduced] = useState(getInitial)

  useEffect(() => {
    const query = window.matchMedia(QUERY)
    const handleChange = () => setReduced(query.matches)
    query.addEventListener("change", handleChange)
    return () => query.removeEventListener("change", handleChange)
  }, [])

  return reduced
}
