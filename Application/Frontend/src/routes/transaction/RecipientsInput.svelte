<script lang="ts">
  import { formatPhoneNumber } from "$lib/phoneNumber";
  import type { User } from "../../models/User";

  let {
    numberId,
    associates,
    recipients = $bindable([]),
  }: {
    numberId: string;
    associates: ReadonlyArray<User>;
    recipients: Array<Recipient>;
  } = $props();

  let phoneNumberElement: HTMLInputElement;

  function addRecipient(user: Recipient): void {
    recipients.push(user);
  }
  function addRecipientFromPhoneNumber(): void {
    try {
      phoneNumberElement.required = true;
      if (!phoneNumberElement.reportValidity()) {
        return;
      }
    } finally {
      phoneNumberElement.required = recipients.length === 0;
    }
    const phoneNumber = formatPhoneNumber(phoneNumberElement.value);
    if (recipients.some((r) => r.phoneNumber === phoneNumber)) {
      return;
    }
    const associate = associates.find((a) => a.phoneNumber === phoneNumber);
    if (associate != null) {
      recipients.push(associate);
    } else {
      recipients.push({ phoneNumber: phoneNumber });
    }
    phoneNumberElement.value = "";
  }
  function removeRecipient(user: Recipient): void {
    const index = recipients.findIndex(
      (r) => r.phoneNumber == user.phoneNumber,
    );
    if (index === -1) return;
    recipients.splice(index, 1);
  }

  type Recipient = Partial<User> & { phoneNumber: string };
</script>

<ul class="recipients">
  {#each recipients as recipient (recipient.phoneNumber)}
    <li>
      <button onclick={() => removeRecipient(recipient)}
        >{recipient.name ?? recipient.phoneNumber}</button
      >
    </li>
  {/each}
</ul>
<input
  id={numberId}
  bind:this={phoneNumberElement}
  type="tel"
  pattern="^\s*(?:\+46|0)(?:(?:\s|\-)?\d){'{'}9}\s*$"
  required={recipients.length === 0}
  placeholder=""
  onkeydowncapture={(e) => {
    if (e.key === "Enter") {
      addRecipientFromPhoneNumber();
      e.preventDefault();
    }
  }}
/>
<button onclick={() => addRecipientFromPhoneNumber()}>Add</button>
{#await associates}
  Loading associates...
{:then associates}
  <ul class="associates">
    {#each associates as associate (associate.id)}
      {#if !recipients.some((r) => r.phoneNumber == associate.phoneNumber)}
        <li>
          <button onclick={() => addRecipient(associate)}
            >{associate.name}</button
          >
        </li>
      {/if}
    {/each}
  </ul>
{:catch e}
  <p>{e.message}</p>
{/await}

<style>
  input:has(+ button) {
    width: calc(100% - (3ch + 1.25rem)) !important;
    margin-inline-end: 0.25rem;
    margin-block: 0.5rem;

    & + button {
      display: inline-block;
      padding: 0.25rem 0.5rem;
      background-color: var(--primary-color);
      color: var(--text-color);
      font-size: 90%;
      text-decoration: none;
      border: none;
      border-radius: 0.25rem;
      text-wrap: nowrap;
    }
  }

  ul {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-wrap: nowrap;
    flex-direction: row;
    justify-content: flex-start;
    overflow: auto hidden;
    overscroll-behavior: contain none;
    gap: 1ch;

    li {
      display: block;

      button {
        display: block;
        padding: 0.25rem 0.5rem;
        background-color: var(--primary-color);
        color: var(--text-color);
        font-size: 90%;
        text-decoration: none;
        border: none;
        border-radius: 0.25rem;
        text-wrap: nowrap;
      }
    }
  }
</style>
