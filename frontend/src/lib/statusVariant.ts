export type BookingLifecycleStatus = "Pending" | "Confirmed" | "Cancelled" | "Completed"

/** Shared by BookingConfirmationPage (customer) and BookingsPage (admin) — both previously
 * hand-rolled this identical status -> badge-variant map. */
export const statusVariant: Record<BookingLifecycleStatus, "warning" | "success" | "error" | "muted"> = {
  Pending: "warning",
  Confirmed: "success",
  Cancelled: "error",
  Completed: "muted",
}
