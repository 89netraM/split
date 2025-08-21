<script lang="ts">
  import "./styles.css";
  import { credentialStore, isValid } from "$lib/auth/credentialStore";
  import { page } from "$app/state";
  import type { Page } from "@sveltejs/kit";
  import type { LayoutParams, RouteId } from "$app/types";

  let { children } = $props();

  function unauthenticated(
    page: Page<LayoutParams<"/">, RouteId | null>,
  ): boolean {
    return page.status !== 200 || page.url.pathname.startsWith("/auth");
  }
</script>

<main>
  {#if unauthenticated(page) || isValid($credentialStore)}
    {@render children()}
  {:else}
    <div>
      {#if $credentialStore != null}
        <p>
          <a
            class="link-button"
            href="/auth/login?redirect={encodeURIComponent(page.url.pathname)}"
            >Login</a
          >
        </p>
      {:else}
        <p>
          <a
            class="link-button"
            href="/auth/register?redirect={encodeURIComponent(
              page.url.pathname,
            )}">Register new device</a
          >
        </p>
      {/if}
    </div>
  {/if}
</main>

{#if isValid($credentialStore)}
  <nav>
    <a href="/" class:active={page.url.pathname === "/"}>Home</a>
    <a href="/balances" class:active={page.url.pathname === "/balances"}
      >Balances</a
    >
  </nav>
{/if}

<style>
  main {
    grid-area: main / main / nav / main;
    overflow: hidden auto;
    overscroll-behavior: none contain;

    div {
      width: 100%;
      height: 100%;
      display: flex;
      justify-content: center;
      align-items: center;
    }
  }

  nav {
    grid-area: nav;
    display: flex;
    flex-wrap: nowrap;
    justify-content: center;
    padding-block: 1rem;
    pointer-events: none;

    a {
      display: inline-block;
      padding: 0.75rem 1.5rem;
      background-color: var(--secondary-color);
      color: var(--text-color);
      text-decoration: none;
      pointer-events: all;

      &.active {
        background-color: var(--primary-color);
      }

      &:first-child {
        border-start-start-radius: 1rem;
        border-end-start-radius: 1rem;
      }

      &:last-child {
        border-start-end-radius: 1rem;
        border-end-end-radius: 1rem;
      }
    }
  }
</style>
