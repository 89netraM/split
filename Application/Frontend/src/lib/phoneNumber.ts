const phoneNumberRegex = /(?:^\+)?\d+/g;

export function formatPhoneNumber(phoneNumber: string): string {
  return [...phoneNumber.matchAll(phoneNumberRegex)].map((m) => m[0]).join("");
}
