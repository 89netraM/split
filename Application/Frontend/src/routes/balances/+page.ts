import { fetchWithToken } from "$lib/auth/fetchWithToken";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch }) => {
  const [balances, me] = await Promise.all([
    fetchWithToken("/api/users/me/balances", {
      fetch,
    }).then((r) => r.json()),
    fetchWithToken("/api/users/me", { fetch }).then((r) => r.json()),
  ]);
  return { balances, me };
};
