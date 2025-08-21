import { writable } from "svelte/store";
import { browser } from "$app/environment";
import { jwtDecode } from "jwt-decode";

const key = "auth";

export interface CredentialStore {
  phoneNumber: string | null;
  token: string | null;
}

export const credentialStore = writable<CredentialStore | null>(
  browser ? JSON.parse(localStorage.getItem(key) ?? "null") : null,
);

credentialStore.subscribe((v) => {
  if (browser) localStorage.setItem(key, JSON.stringify(v));
});

export function isValid(store: CredentialStore | null): boolean {
  if (store?.token == null) return false;
  const decoded = jwtDecode(store.token);
  return decoded.exp != null && Date.now() < decoded.exp * 1000;
}
