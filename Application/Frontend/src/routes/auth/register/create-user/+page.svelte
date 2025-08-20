<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";
  import { credentialStore } from "$lib/auth/credentialStore";

  const redirect = page.url.searchParams.get("redirect");

  if (
    !(
      "phoneNumber" in page.state &&
      "context" in page.state &&
      "options" in page.state
    )
  ) {
    let nextPath = "/auth/register";
    if (redirect != null) {
      nextPath += "?redirect=" + encodeURIComponent(redirect);
    }
    goto(nextPath);
  }
  const { phoneNumber, context, options } = page.state as {
    phoneNumber: string;
    context: string;
    options: PublicKeyCredentialCreationOptionsJSON;
  };
  const publicKey = PublicKeyCredential.parseCreationOptionsFromJSON(options);

  let userName = $state("");
  let action: Promise<void> | null = $state(null);

  function onsubmit() {
    action = (async () => {
      const attestation = await navigator.credentials.create({ publicKey });
      if (!(attestation instanceof PublicKeyCredential)) {
        throw new Error("Not public key credentials");
      }

      const { token } = await fetch(`/api/auth/credential/new`, {
        method: "POST",
        body: JSON.stringify({
          attestation,
          challengeContext: context,
          userName,
        }),
        mode: "cors",
        headers: {
          "Content-Type": "application/json",
        },
      }).then((r) => r.json());
      credentialStore.set({
        phoneNumber,
        token,
      });
      goto(redirect ?? "/");
    })();
  }
</script>

{#if action == null}
  <form {onsubmit}>
    <label>
      Name:
      <input bind:value={userName} required />
    </label>
    <button type="submit">Create user</button>
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
