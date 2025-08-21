<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";
  import { credentialStore } from "$lib/auth/credentialStore";
  import { formatPhoneNumber } from "$lib/phoneNumber";

  const redirect = page.url.searchParams.get("redirect");

  let phoneNumber = $state($credentialStore?.phoneNumber ?? "");
  let savePhoneNumber = $state($credentialStore?.phoneNumber != null);
  let action: Promise<void> | null = $state(null);

  function onsubmit() {
    action = (async () => {
      const formattedPhoneNumber = formatPhoneNumber(phoneNumber);
      const { assertion, assertionContext } = await fetch(
        `/api/auth/assertion?phoneNumber=${encodeURIComponent(formattedPhoneNumber)}`,
      ).then((r) => r.json());
      const assertionResult = await navigator.credentials.get({
        publicKey: PublicKeyCredential.parseRequestOptionsFromJSON(assertion),
      });
      if (!(assertionResult instanceof PublicKeyCredential)) {
        throw new Error("Not public key credentials");
      }
      const { token } = await fetch("/api/auth/assertion", {
        method: "POST",
        body: JSON.stringify({
          assertion: assertionResult,
          context: assertionContext,
        }),
        mode: "cors",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((r) => r.json());
      credentialStore.set({
        phoneNumber: savePhoneNumber ? phoneNumber : null,
        token,
      });
      goto(redirect ?? "/");
    })();
  }
</script>

<svelte:head><title>Split - Login</title></svelte:head>

<div class="simple-form">
  <h2>Login</h2>
  {#if action == null}
    <form {onsubmit}>
      <p>
        <label>
          <span>Phone number:</span>
          <input
            type="tel"
            bind:value={phoneNumber}
            pattern="^\s*(?:\+46|0)(?:(?:\s|\-)?\d){'{'}9}\s*$"
            required
            placeholder=""
          />
        </label>
      </p>
      <p>
        <label>
          <span>Save phone number:</span>
          <input type="checkbox" bind:checked={savePhoneNumber} />
        </label>
      </p>
      <p>
        <button type="submit">Login with a Passkey</button>
      </p>
    </form>
  {:else}
    {#await action}
      <p>Loading...</p>
    {:then}
      <p>Redirecting...</p>
    {:catch e}
      <p>{e.message}</p>
    {/await}
  {/if}
</div>
