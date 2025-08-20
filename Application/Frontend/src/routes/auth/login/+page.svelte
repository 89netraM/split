<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";
  import { credentialStore } from "$lib/auth/credentialStore";

  const redirect = page.url.searchParams.get("redirect");

  let phoneNumber = $state($credentialStore?.phoneNumber ?? "");
  let savePhoneNumber = $state($credentialStore?.phoneNumber != null);
  let action: Promise<void> | null = $state(null);

  function onsubmit() {
    action = (async () => {
      const { assertion, assertionContext } = await fetch(
        `/api/auth/assertion?phoneNumber=${encodeURIComponent(phoneNumber)}`,
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
      Save phone number:
      <input type="checkbox" bind:checked={savePhoneNumber} />
    </label>
    <button type="submit">Login with a Passkey</button>
  </form>
{:else}
  {#await action}
    Loading...
  {:then}
    Redirecting...
  {:catch e}
    <p>{e.message}</p>
  {/await}
{/if}
