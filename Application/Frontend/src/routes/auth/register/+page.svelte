<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";

  const redirect = page.url.searchParams.get("redirect");

  let phoneNumber = $state("");
  let savePhoneNumber = $state(false);
  let action: Promise<void> | null = $state(null);

  function onsubmit() {
    action = (async () => {
      const { context } = await fetch(
        `/api/auth/verify-phone-number?phoneNumber=${encodeURIComponent(phoneNumber)}`,
      ).then((r) => r.json());
      let nextPath = "/auth/register/verify-phone-number";
      if (redirect != null) {
        nextPath += "?redirect=" + encodeURIComponent(redirect);
      }
      goto(nextPath, {
        state: {
          phoneNumber: savePhoneNumber ? phoneNumber : null,
          phoneNumberVerificationContext: context,
        },
      });
    })();
  }
</script>

{#if action == null}
  <form {onsubmit}>
    <label>
      Phone number:
      <input
        type="tel"
        bind:value={phoneNumber}
        pattern="^\+467\d{'{'}8}$"
        required
        placeholder="+467XXXXXXXX"
      />
    </label>
    <label>
      <input type="checkbox" bind:checked={savePhoneNumber} />
      Save phone number
    </label>
    <button type="submit">Request verification code</button>
  </form>
{:else}
  {#await action}
    Sending code...
  {:then}
    Redirecting...
  {:catch e}
    <p>{e.message}</p>
  {/await}
{/if}
