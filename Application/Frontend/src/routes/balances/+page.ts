import { fetchWithToken } from "$lib/auth/fetchWithToken";
import type { Balance } from "../../models/Balance.js";
import type { User } from "../../models/User.js";

export async function load({ fetch }): Promise<{
  balances: ReadonlyArray<Balance>;
  me: User;
}> {
  const [balances, me] = await Promise.all([
    fetchWithToken("/api/users/me/balances", {
      fetch,
    }).then((r) => r.json()),
    fetchWithToken("/api/users/me", { fetch }).then((r) => r.json()),
  ]);
  return { balances, me };
}
