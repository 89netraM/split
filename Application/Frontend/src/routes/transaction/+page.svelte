<script lang="ts">
  import { fetchWithToken } from "$lib/auth/fetchWithToken";
  import RecipientsInput from "./RecipientsInput.svelte";

  let { data } = $props();

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

<svelte:head><title>Split - New Transaction</title></svelte:head>

<div class="simple-form">
  <h2>New Transaction</h2>
  {#if action == null}
    <form {onsubmit} onkeydowncapture={preventSubmitOnEnter}>
      <p>
        <label for="recipients"><span>Recipients</span></label>
        <RecipientsInput
          numberId="recipients"
          associates={data.associates}
          bind:recipients
        />
      </p>
      <p>
        <label>
          <span>Amount</span>
          <input
            type="number"
            min="0"
            required
            placeholder=""
            bind:value={amount}
          />
        </label>
      </p>
      <p>
        <label>
          <span>Description</span>
          <input type="text" placeholder="" bind:value={description} />
        </label>
      </p>
      <p>
        <button type="submit">Send</button>
      </p>
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
</div>
