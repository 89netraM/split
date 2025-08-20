<script lang="ts">
  import { page } from "$app/state";
  import type { User } from "../../models/User";

  let { numberId, recipients = $bindable([]) } = $props();

  let associates: ReadonlyArray<User> = page.data.associates;
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
    if (recipients.some((r) => r.phoneNumber === phoneNumberElement.value)) {
      return;
    }
    const associate = associates.find(
      (a) => a.phoneNumber === phoneNumberElement.value,
    );
    if (associate != null) {
      recipients.push(associate);
    } else {
      recipients.push({ phoneNumber: phoneNumberElement.value });
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

<ul>
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
  pattern="^\+467\d{'{'}8}$"
  required={recipients.length === 0}
  placeholder="+467XXXXXXXX"
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
  <ul>
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
