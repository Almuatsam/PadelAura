import { createContext, useContext } from "react"

/**
 * Squash-and-stretch tap intensity for interactive primitives (Button, Card).
 * AdminLayout provides a lower value so staff-facing screens share the exact
 * same components at a calmer intensity, instead of forking variants.
 */
export const BounceScaleContext = createContext(1)

export function useBounceScale(): number {
  return useContext(BounceScaleContext)
}
