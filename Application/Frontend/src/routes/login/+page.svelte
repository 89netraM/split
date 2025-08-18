<script lang="ts">
  let phoneNumber = $state("");
  let token: string | null = $state(null);

  async function onsubmit() {
    const { assertion, assertionContext } = await fetch(
      `/api/auth/assertion?phoneNumber=${encodeURIComponent(phoneNumber)}`,
    ).then((r) => r.json());
    const assertionResult = await navigator.credentials.get({
      publicKey: PublicKeyCredential.parseRequestOptionsFromJSON(assertion),
    });
    if (!(assertionResult instanceof PublicKeyCredential)) {
      alert("Not public key credentials");
      return;
    }
    const { token: t } = await fetch("/api/auth/assertion", {
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
    token = t;
  }
</script>

{#if token == null}
  <form {onsubmit}>
    <label>
      <input
        type="tel"
        bind:value={phoneNumber}
        pattern="^\+467\d{'{'}8}$"
        required
        placeholder="+467XXXXXXXX"
      />
    </label>
    <button type="submit">Login with a Passkey</button>
  </form>
{:else}
  <p>Your token: <code>{token}</code></p>
{/if}
