import { fetchWithToken } from "$lib/auth/fetchWithToken";
import type { User } from "../../models/User";
import type { PageLoad } from "./$types";

export const ssr = false;

export const load: PageLoad = async ({ fetch }) => {
  const associates: ReadonlyArray<User> = await Promise.all([
    fetchWithToken("/api/users/me", { fetch }),
    fetchWithToken("/api/users/me/associates", { fetch }),
  ])
    .then(([me, associates]) => Promise.all([me.json(), associates.json()]))
    .then(([me, associates]) => [me, ...associates]);
  return { associates };
};
