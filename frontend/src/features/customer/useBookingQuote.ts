import { useQuery } from "@tanstack/react-query"

import { fetchBookingQuote } from "@/features/customer/api/quote"
import type { CartSlot } from "@/features/customer/cart"

/**
 * The authoritative, promotion-adjusted total for the current cart — the same computation
 * CreateBookingCommandHandler uses to charge the booking, so the customer never sees a total on
 * the website that differs from what's actually sent to Thawani.
 */
export function useBookingQuote(cart: CartSlot[]) {
  const slots = [...cart]
    .map(({ date, startTime, endTime }) => ({ date, startTime, endTime }))
    .sort((a, b) => (a.date + a.startTime).localeCompare(b.date + b.startTime))

  const { data: quote, isLoading, isError } = useQuery({
    queryKey: ["booking-quote", slots],
    queryFn: () => fetchBookingQuote(slots),
    enabled: slots.length > 0,
  })

  return { quote, isLoading, isError }
}
