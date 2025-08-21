import { credentialStore, isValid } from "$lib/auth/credentialStore";
import { fetchWithToken } from "$lib/auth/fetchWithToken";
import { get } from "svelte/store";
import type { User } from "../../models/User";

export const ssr = false;

export async function load({
  fetch,
}): Promise<{ associates: ReadonlyArray<User> }> {
  if (!isValid(get(credentialStore))) {
    return { associates: [] };
  }
  const associates: ReadonlyArray<User> = await Promise.all([
    fetchWithToken("/api/users/me", { fetch }),
    fetchWithToken("/api/users/me/associates", { fetch }),
  ])
    .then(([me, associates]) => Promise.all([me.json(), associates.json()]))
    .then(([me, associates]) => [me, ...associates]);
  return { associates };
}
