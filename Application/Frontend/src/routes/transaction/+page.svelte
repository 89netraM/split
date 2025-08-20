<script lang="ts">
  import { page } from "$app/state";
  import { fetchWithToken } from "$lib/auth/fetchWithToken";
  import RecipientsInput from "./RecipientsInput.svelte";

  let action: Promise<void> | null = $state(null);

  let recipients: Array<Recipient> = $state([]);
  let amount: number | null = $state(null);
  let description = $state("");

  function onsubmit(): void {
    action = (async () => {
      const response = await fetchWithToken("/api/transactions", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          recipients,
          amount: {
            amount,
            currency: "SEK",
          },
          description,
        }),
      });
      if (response.ok) return;
      const errorMessage = await response.text();
      throw new Error(errorMessage);
    })();
  }

  function preventSubmitOnEnter(e: KeyboardEvent): void {
    if (e.key !== "Enter") return;
    if (e.target instanceof HTMLButtonElement) return;
    e.preventDefault();
  }

  function reset(): void {
    action = null;
    recipients = [];
    amount = null;
    description = "";
  }

  interface Recipient {
    phoneNumber: string;
  }
</script>

<h2>New Transaction</h2>

{#if action == null}
  <form {onsubmit} onkeydowncapture={preventSubmitOnEnter}>
    <label for="recipients">Recipients</label>
    <RecipientsInput
      numberId="recipients"
      associates={page.data.associates}
      bind:recipients
    />
    <label>
      Amount
      <input type="number" min="0" required bind:value={amount} />
    </label>
    <label>
      Description
      <input type="text" bind:value={description} />
    </label>
    <button type="submit">Send</button>
  </form>
{:else}
  {#await action}
    <p>Sending transaction...</p>
  {:then}
    <p>Transaction sent</p>
    <p><a href="/transaction" onclick={() => reset()}>New Transaction</a></p>
    <p><a href="/">Back to Home</a></p>
  {:catch e}
    <p>{e.message}</p>
  {/await}
{/if}
