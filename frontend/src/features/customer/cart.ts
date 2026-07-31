export type CartSlot = {
  date: string
  startTime: string
  endTime: string
  price: number
}

export function slotKey(date: string, startTime: string): string {
  return `${date}_${startTime}`
}

export function cartTotal(cart: CartSlot[]): number {
  return cart.reduce((sum, slot) => sum + slot.price, 0)
}
