import { useState } from "react"

import { usePrefersReducedMotion } from "@/lib/usePrefersReducedMotion"

// Scoped, prefixed keyframes so this stays a single drop-in component with no
// external animation library and no coupling to index.css. Colors are the site's
// existing CSS variables (navy/cream/orange/amber) — no hardcoded hex, no purple.
const STYLE = `
.pra-swing {
  transform-box: view-box;
  transform-origin: 0 0;
  animation: pra-swing 5s cubic-bezier(.34,1.1,.4,1) infinite;
}
.pra-ball {
  transform-box: view-box;
  transform-origin: 130px 138px;
  animation: pra-ball 5s cubic-bezier(.22,.8,.3,1) infinite;
}
.pra-trail {
  transform-box: view-box;
  transform-origin: 130px 138px;
  animation: pra-trail 5s ease-out infinite;
}
.pra-flash {
  transform-box: view-box;
  transform-origin: 130px 138px;
  animation: pra-flash 5s ease-out infinite;
}
@keyframes pra-swing {
  0%     { transform: translate(-190px,-120px) rotate(-75deg); opacity: 0; }
  8%     { transform: translate(-190px,-120px) rotate(-75deg); opacity: 1; }
  24%    { transform: translate(-10px,-6px) rotate(-8deg); opacity: 1; }
  28%    { transform: translate(0,0) rotate(0deg); opacity: 1; }
  34%    { transform: translate(14px,6px) rotate(10deg); opacity: 1; }
  46%    { transform: translate(30px,10px) rotate(16deg); opacity: 0; }
  100%   { transform: translate(-190px,-120px) rotate(-75deg); opacity: 0; }
}
/* The ball stays visible at rest for almost the whole cycle — only the ~1% window
   around the reset snap (46-47%) is invisible, so the hero never reads as "empty". */
@keyframes pra-ball {
  0%     { transform: translate(0,0) scale(1); opacity: 1; }
  26%    { transform: translate(0,0) scale(1); opacity: 1; }
  30%    { transform: translate(8px,-6px) scale(1.05); opacity: 1; }
  46%    { transform: translate(130px,-80px) scale(.5); opacity: 0; }
  47%    { transform: translate(0,0) scale(1); opacity: 0; }
  53%    { transform: translate(0,0) scale(1); opacity: 1; }
  100%   { transform: translate(0,0) scale(1); opacity: 1; }
}
@keyframes pra-trail {
  0%     { opacity: 0; transform: translate(0,0) scaleX(.3); }
  29%    { opacity: 0; transform: translate(0,0) scaleX(.3); }
  32%    { opacity: .55; transform: translate(30px,-18px) scaleX(1); }
  46%    { opacity: 0; transform: translate(90px,-55px) scaleX(1.4); }
  100%   { opacity: 0; transform: translate(0,0) scaleX(.3); }
}
@keyframes pra-flash {
  0%     { opacity: 0; transform: scale(.4); }
  27%    { opacity: 0; transform: scale(.4); }
  29%    { opacity: .9; transform: scale(1.3); }
  34%    { opacity: 0; transform: scale(1.8); }
  100%   { opacity: 0; transform: scale(.4); }
}
@media (prefers-reduced-motion: reduce) {
  .pra-swing, .pra-ball, .pra-trail, .pra-flash { animation: none; }
}
`

function RacketMarkup() {
  return (
    <g transform="translate(128,118) rotate(18)">
      <rect x="-4" y="28" width="8" height="25" rx="4" fill="var(--foreground)" />
      <ellipse cx="0" cy="-6" rx="24" ry="32" fill="var(--card)" stroke="var(--foreground)" strokeWidth="5" />
      <clipPath id="pra-string-clip">
        <ellipse cx="0" cy="-6" rx="20.5" ry="28.5" />
      </clipPath>
      <g clipPath="url(#pra-string-clip)" stroke="var(--primary)" strokeWidth="1.1" opacity="0.8">
        <line x1="-16" y1="-34" x2="-16" y2="22" />
        <line x1="-8" y1="-34" x2="-8" y2="22" />
        <line x1="0" y1="-34" x2="0" y2="22" />
        <line x1="8" y1="-34" x2="8" y2="22" />
        <line x1="16" y1="-34" x2="16" y2="22" />
        <line x1="-20" y1="-24" x2="20" y2="-24" />
        <line x1="-20" y1="-14" x2="20" y2="-14" />
        <line x1="-20" y1="-4" x2="20" y2="-4" />
        <line x1="-20" y1="6" x2="20" y2="6" />
        <line x1="-20" y1="16" x2="20" y2="16" />
      </g>
    </g>
  )
}

function BallMarkup() {
  return (
    <g>
      <circle cx="130" cy="138" r="13" fill="url(#pra-ball-glow)" />
      <path
        d="M118,134 Q130,146 142,134"
        stroke="var(--foreground)"
        strokeWidth="1.4"
        fill="none"
        opacity="0.55"
      />
      <path
        d="M118,142 Q130,130 142,142"
        stroke="var(--foreground)"
        strokeWidth="1.4"
        fill="none"
        opacity="0.55"
      />
    </g>
  )
}

type HeroRacketStrikeProps = {
  className?: string
}

export function HeroRacketStrike({ className }: HeroRacketStrikeProps) {
  const prefersReducedMotion = usePrefersReducedMotion()
  const [playKey, setPlayKey] = useState(0)
  const replay = () => setPlayKey((key) => key + 1)

  return (
    <div className={className} aria-hidden="true" onClick={replay} onMouseEnter={replay}>
      <style>{STYLE}</style>
      <svg viewBox="0 0 220 220" className="h-auto w-full">
        <defs>
          <radialGradient id="pra-ball-glow" cx="35%" cy="32%" r="70%">
            <stop offset="0%" stopColor="var(--secondary)" />
            <stop offset="100%" stopColor="var(--primary)" />
          </radialGradient>
        </defs>

        {prefersReducedMotion ? (
          <g>
            <RacketMarkup />
            <ellipse
              cx="175"
              cy="88"
              rx="26"
              ry="8"
              fill="var(--secondary)"
              opacity="0.3"
              transform="rotate(-30 175 88)"
            />
            <g transform="translate(45,-30)">
              <BallMarkup />
            </g>
            <circle cx="130" cy="138" r="6" fill="var(--secondary)" opacity="0.35" />
          </g>
        ) : (
          <g key={playKey}>
            <ellipse className="pra-trail" cx="130" cy="138" rx="26" ry="8" fill="var(--secondary)" transform="rotate(-30 130 138)" />
            <g className="pra-swing">
              <RacketMarkup />
            </g>
            <circle className="pra-flash" cx="130" cy="138" r="16" fill="var(--secondary)" />
            <g className="pra-ball">
              <BallMarkup />
            </g>
          </g>
        )}
      </svg>
    </div>
  )
}
