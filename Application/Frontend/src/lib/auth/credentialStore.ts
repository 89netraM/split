import { writable } from "svelte/store";
import { browser } from "$app/environment";

const key = "auth";

export interface CredentialStore {
  phoneNumber: string | null;
  token: string;
}

export const credentialStore = writable<CredentialStore | null>(
  browser ? JSON.parse(localStorage.getItem(key) ?? "null") : null,
);

credentialStore.subscribe((v) => {
  if (browser) localStorage.setItem(key, JSON.stringify(v));
});
