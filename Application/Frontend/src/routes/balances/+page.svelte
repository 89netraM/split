<script lang="ts">
  import { goto } from "$app/navigation";
  import type { Balance } from "../../models/Balance";

  let { data } = $props();

  function settleDebt(balance: Balance): void {
    goto("/balances/settle", { state: { balance } });
  }
</script>

<h2>Balances</h2>

{#each data.balances as balance ([balance.from.id, balance.to.id]
  .sort()
  .join("<key-center>"))}
  <span>
    {#if balance.from.id === data.me.id}
      <strong>{balance.from.name}</strong>
    {:else}
      {balance.from.name}
    {/if}
  </span>
  <span>
    {#if balance.to.id === data.me.id}
      <strong>{balance.to.name}</strong>
    {:else}
      {balance.to.name}
    {/if}
  </span>
  <span>{balance.amount.amount} {balance.amount.currency}</span>
  {#if balance.from.id !== data.me.id && balance.amount.amount > 0 && balance.amount.currency === "SEK"}
    <a
      href="/balance/settle"
      onclick={(e) => (settleDebt(balance), e.preventDefault())}>Settle</a
    >
  {/if}
{:else}
  <p>No balances yet</p>
  <p>Go <a href="/transaction">make a transaction</a></p>
{/each}
