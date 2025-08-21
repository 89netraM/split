import { get } from "svelte/store";
import { credentialStore } from "./credentialStore";

export async function fetchWithToken(
  input: RequestInfo | URL,
  init?: RequestInit & {
    fetch?: (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>;
  },
): Promise<Response> {
  const fetch = init?.fetch ?? window.fetch;
  const response = await fetch(input, {
    ...init,
    headers: {
      ...init?.headers,
      Authorization: "Bearer " + get(credentialStore)?.token,
    },
  });
  if (response.status === 401) {
    credentialStore.update((c) => ({
      phoneNumber: c?.phoneNumber ?? null,
      token: null,
    }));
  }
  return response;
}
