<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";
  import { fetchWithToken } from "$lib/auth/fetchWithToken";
  import { createSwishLink } from "$lib/swish";
  import type { Balance } from "../../../models/Balance";

  if (!("balance" in page.state)) {
    goto("/balances");
  }
  const { balance } = page.state as { balance: Balance };

  let action: Promise<void> | null = $state(null);

  window.location.assign(
    createSwishLink(balance.amount.amount, balance.from.id),
  );

  function confirmSwish(): void {
    action = (async () => {
      const response = await fetchWithToken("/api/transactions", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          recipients: [{ id: balance.from.id }],
          amount: balance.amount,
          description: "Settling debt",
        }),
      });
      if (response.ok) {
        goto("/balances");
        return;
      }
      const errorMessage = await response.text();
      throw new Error(errorMessage);
    })();
  }
</script>

<div>
  <h2>Confirm Swish</h2>
  <div>
    <p>Have you sent the Swish payment?</p>
    {#if action != null}
      {#await action}
        <p>Settling debt...</p>
      {:then}
        <p>Redirecting debt...</p>
      {:catch e}
        <p>{e.message}</p>
      {/await}
    {:else}
      <p>
        <a
          class="link-button"
          href="/balances"
          data-sveltekit-preload-data="false"
          onclick={(e) => (confirmSwish(), e.preventDefault())}>Yes</a
        >
        <a
          class="link-button no"
          href="/balances"
          data-sveltekit-preload-data="false">No</a
        >
      </p>
    {/if}
  </div>
</div>

<style>
  :global(main) > div {
    width: 100%;
    min-height: 100%;
    display: grid;
    grid-template-rows: 1fr 3fr;

    h2 {
      grid-row: 1;
      grid-column: 1;
      justify-self: start;
      align-self: end;
      margin: 0 1rem;
      color: var(--text-color);
    }

    div {
      grid-row: 2;
      grid-column: 1;

      p {
        padding-inline: 3rem;
        color: var(--text-color);

        &:has(a) {
          text-align: center;
        }

        a.no {
          background-color: var(--error-color);
        }
      }
    }
  }
</style>
