<script lang="ts">
  import { goto } from "$app/navigation";
  import { page } from "$app/state";
  import { credentialStore } from "$lib/auth/credentialStore";

  const redirect = page.url.searchParams.get("redirect");

  if (
    !(
      "phoneNumber" in page.state &&
      "phoneNumberVerificationContext" in page.state
    )
  ) {
    let nextPath = "/auth/register";
    if (redirect != null) {
      nextPath += "?redirect=" + encodeURIComponent(redirect);
    }
    goto(nextPath);
  }
  const { phoneNumber, phoneNumberVerificationContext } = page.state as {
    phoneNumber: string;
    phoneNumberVerificationContext: string;
  };

  let phoneNumberVerificationCode = $state("");
  let action: Promise<void> | null = $state(null);

  function onsubmit() {
    action = (async () => {
      const { context, options, userExists } = await fetch(
        `/api/auth/credential`,
        {
          method: "POST",
          body: JSON.stringify({
            phoneNumberVerificationCode,
            phoneNumberVerificationContext,
          }),
          mode: "cors",
          headers: {
            "Content-Type": "application/json",
          },
        },
      ).then((r) => r.json());

      if (!userExists) {
        let nextPath = "/auth/register/create-user";
        if (redirect != null) {
          nextPath += "?redirect=" + encodeURIComponent(redirect);
        }
        goto(nextPath, {
          state: { phoneNumber, context, options },
        });
        return;
      }

      const publicKey =
        PublicKeyCredential.parseCreationOptionsFromJSON(options);

      const attestation = await navigator.credentials.create({ publicKey });
      if (!(attestation instanceof PublicKeyCredential)) {
        throw new Error("Not public key credentials");
      }

      const { token } = await fetch(`/api/auth/credential/existing`, {
        method: "POST",
        body: JSON.stringify({
          attestation,
          challengeContext: context,
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
      Verification code:
      <input
        bind:value={phoneNumberVerificationCode}
        autocomplete="one-time-code"
        required
      />
    </label>
    <button type="submit">Verify phone number</button>
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
