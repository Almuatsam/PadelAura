import { useEffect, useRef, useState } from "react"
import { motion } from "framer-motion"

import { PadelBall } from "@/components/illustrations/PadelBall"
import { usePrefersReducedMotion } from "@/lib/usePrefersReducedMotion"

type Particle = {
  id: number
  angle: number
  distance: number
  rotate: number
  shape: "ball" | "star"
  color: string
}

const COLORS = ["#F25C05", "#FFC145", "#22223B"]
const BURST_DURATION_MS = 1300

function createParticles(count: number): Particle[] {
  return Array.from({ length: count }, (_, index) => ({
    id: index,
    angle: Math.random() * Math.PI * 2,
    distance: 60 + Math.random() * 60,
    rotate: Math.random() * 360 - 180,
    shape: Math.random() > 0.5 ? "ball" : "star",
    color: COLORS[index % COLORS.length],
  }))
}

type ConfettiBurstProps = {
  /** Fires a burst on the transition from false -> true. Render inside a `relative` parent. */
  trigger: boolean
  particleCount?: number
}

export function ConfettiBurst({ trigger, particleCount = 18 }: ConfettiBurstProps) {
  const prefersReducedMotion = usePrefersReducedMotion()
  const [particles, setParticles] = useState<Particle[]>([])
  const firedRef = useRef(false)

  useEffect(() => {
    if (!trigger) {
      firedRef.current = false
      return
    }
    if (firedRef.current || prefersReducedMotion) return
    firedRef.current = true
    setParticles(createParticles(particleCount))
    const timeout = setTimeout(() => setParticles([]), BURST_DURATION_MS)
    return () => clearTimeout(timeout)
  }, [trigger, particleCount, prefersReducedMotion])

  if (particles.length === 0) return null

  return (
    <div className="pointer-events-none absolute inset-0 z-10" aria-hidden="true">
      {particles.map((particle) => {
        const x = Math.cos(particle.angle) * particle.distance
        const y = Math.sin(particle.angle) * particle.distance
        return (
          <motion.div
            key={particle.id}
            className="absolute top-1/2 left-1/2 size-4"
            style={{ color: particle.color }}
            initial={{ x: 0, y: 0, opacity: 1, rotate: 0, scale: 0.6 }}
            animate={{ x, y, opacity: 0, rotate: particle.rotate, scale: 1 }}
            transition={{ duration: 1.1, ease: "easeOut" }}
          >
            {particle.shape === "ball" ? (
              <PadelBall className="size-full" />
            ) : (
              <svg viewBox="0 0 24 24" className="size-full" fill="currentColor">
                <path d="M12 2 L14.5 9 L22 9.5 L16 14.5 L18 22 L12 17.5 L6 22 L8 14.5 L2 9.5 L9.5 9 Z" />
              </svg>
            )}
          </motion.div>
        )
      })}
    </div>
  )
}
