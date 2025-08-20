<script lang="ts">
  import { page } from "$app/state";
  import type { Balance } from "../../models/Balance";
  import type { User } from "../../models/User";

  const me: User = page.data.me;
  const balances: ReadonlyArray<Balance> = page.data.balances;
</script>

<svelte:head><title>Split - Balances</title></svelte:head>

<h2>Balances</h2>

{#each balances as balance ([balance.from.id, balance.to.id]
  .sort()
  .join("<key-center>"))}
  <span>
    {#if balance.from.id === me.id}
      <strong>{balance.from.name}</strong>
    {:else}
      {balance.from.name}
    {/if}
  </span>
  <span>
    {#if balance.to.id === me.id}
      <strong>{balance.to.name}</strong>
    {:else}
      {balance.to.name}
    {/if}
  </span>
  <span>{balance.amount.amount} {balance.amount.currency}</span>
{:else}
  <p>No balances yet</p>
  <p>Go <a href="/transaction">make a transaction</a></p>
{/each}
