<script lang="ts">
  import { goto } from "$app/navigation";
  import type { Balance } from "../../models/Balance";

  let { data } = $props();

  function settleDebt(balance: Balance): void {
    goto("/balances/settle", { state: { balance } });
  }
</script>

<div class="container">
  <h2>Balances</h2>

  <div class="balances">
    {#if data.balances.length > 0}
      <span>From</span>
      <span>Amount</span>
      <span>To</span>
    {/if}
    {#each data.balances as balance ([balance.from.id, balance.to.id]
      .sort()
      .join("<key-center>"))}
      <span class="from">
        {#if balance.from.id === data.me?.id}
          <strong>{balance.from.name}</strong>
        {:else}
          {balance.from.name}
        {/if}
      </span>
      <span class="amount">
        <span class="currency">{balance.amount.currency}</span>&nbsp;{Number(
          balance.amount.amount,
        ).toFixed(2)}
      </span>
      <span class="to">
        {#if balance.to.id === data.me?.id}
          <strong>{balance.to.name}</strong>
        {:else}
          {balance.to.name}
        {/if}
      </span>
      {#if balance.from.id !== data.me?.id && balance.amount.amount > 0 && balance.amount.currency === "SEK"}
        <a
          class="settle"
          href="/balance/settle"
          onclick={(e) => (settleDebt(balance), e.preventDefault())}>Settle</a
        >
      {/if}
    {:else}
      <p>No balances yet</p>
      <p>Go <a href="/transaction">make a transaction</a></p>
    {/each}
  </div>
</div>

<style>
  .container {
    width: 100%;
    min-height: 100%;
    display: grid;
    grid-template-rows: 1fr 3fr;

    h2 {
      grid-row: 1;
      grid-column: 1;
      justify-self: flex-start;
      align-self: flex-end;
      margin: 0 1rem;
      color: var(--text-color);
    }

    .balances {
      grid-row: 2;
      grid-column: 1;
      justify-self: stretch;
      align-self: flex-start;
      display: grid;
      padding: 1rem;
      grid-template-columns: 1fr auto 1fr;
      gap: 0 0.5rem;
      color: var(--text-color);

      .from {
        grid-column: 1;
        margin-block-start: 0.5rem;
      }

      .amount {
        grid-column: 2;
        margin-block-start: 0.5rem;
        font-variant-numeric: tabular-nums;
        text-align: end;

        .currency {
          font-size: 80%;
        }
      }

      .to {
        grid-column: 3;
        margin-block-start: 0.5rem;
      }

      .settle {
        grid-column: 1 / span 3;
        justify-self: end;
        background: var(--primary-color);
        color: var(--text-color);
        font-size: 90%;
        padding: 0.5rem 1rem;
        border: none;
        border-radius: 0.5rem;
        margin-block-start: 0.25rem;
        text-decoration: none;
      }

      @media (min-width: 600px) {
        grid-template-columns: auto auto auto auto;
        gap: 0 1rem;
        grid-auto-rows: 2rem;
        align-items: center;
        justify-content: center;

        .from {
          grid-column: 1;
          margin-block-start: unset;
        }

        .amount {
          grid-column: 2;
          margin-block-start: unset;
        }

        .to {
          grid-column: 3;
          margin-block-start: unset;
        }

        .settle {
          grid-column: 4;
          justify-self: center;
          margin-block-start: unset;
        }
      }

      p {
        grid-column: 2;
        margin: 0;
      }
    }
  }
</style>
