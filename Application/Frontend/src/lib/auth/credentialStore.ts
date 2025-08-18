import { writable } from "svelte/store";

const key = "auth";

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface CredentialStore {}

export const credentialStore = writable<CredentialStore | null>(
  JSON.parse(localStorage.getItem(key) ?? "null"),
);

credentialStore.subscribe((v) => localStorage.setItem(key, JSON.stringify(v)));
