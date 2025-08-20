import { get } from "svelte/store";
import { credentialStore } from "./credentialStore";

export function fetchWithToken(
  input: RequestInfo | URL,
  init?: RequestInit & {
    fetch?: (input: RequestInfo | URL, init?: RequestInit) => Promise<Response>;
  },
): Promise<Response> {
  const fetch = init?.fetch ?? window.fetch;
  return fetch(input, {
    ...init,
    headers: {
      ...init?.headers,
      Authorization: "Bearer " + get(credentialStore)?.token,
    },
  });
}
