<script lang="ts">
  import { jwtDecode } from "jwt-decode";
  import {
    type CredentialStore,
    credentialStore,
  } from "$lib/auth/credentialStore";
  import { page } from "$app/state";
  import type { Page } from "@sveltejs/kit";
  import type { LayoutParams, RouteId } from "$app/types";

  let { children } = $props();

  function unauthenticated(
    page: Page<LayoutParams<"/">, RouteId | null>,
  ): boolean {
    return page.status !== 200 || page.url.pathname.startsWith("/auth");
  }

  function isValid(store: CredentialStore | null): boolean {
    if (store?.token == null) return false;
    const decoded = jwtDecode(store.token);
    return decoded.exp != null && Date.now() < decoded.exp * 1000;
  }
</script>

<h1>Split</h1>

{#if unauthenticated(page) || isValid($credentialStore)}
  {@render children()}
{:else if $credentialStore != null}
  <p>
    <a href="/auth/login?redirect={encodeURIComponent(page.url.pathname)}"
      >Login</a
    >
  </p>
{:else}
  <p>
    <a href="/auth/register?redirect={encodeURIComponent(page.url.pathname)}"
      >Register new device</a
    >
  </p>
{/if}
