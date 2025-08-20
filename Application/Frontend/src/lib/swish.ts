export function createSwishLink(kronor: number, phoneNumber: string): URL {
  return new URL(
    `swish://payment?data=${encodeURIComponent(
      JSON.stringify({
        payee: { value: phoneNumber },
        amount: { value: kronor },
        version: 1,
      }),
    )}`,
  );
}
