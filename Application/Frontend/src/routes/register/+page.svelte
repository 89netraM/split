<script lang="ts">
  let phoneNumber = $state("");
  let phoneNumberVerificationContext: string | null = $state(null);

  let phoneNumberVerificationCode = $state("");
  let challengeContext: string | null = $state(null);
  let publicKey: PublicKeyCredentialCreationOptions | null = null;

  let userName = $state("");
  let token: string | null = $state(null);

  async function onSubmitPhoneNumber(e: SubmitEvent) {
    const { context } = await fetch(
      `http://localhost:5223/api/auth/verify-phone-number?phoneNumber=${encodeURIComponent(phoneNumber)}`,
    ).then((r) => r.json());
    phoneNumberVerificationContext = context;
  }

  async function onSubmitVerification(e: SubmitEvent) {
    const { context, options, userExists } = await fetch(
      `http://localhost:5223/api/auth/credential`,
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
    publicKey = PublicKeyCredential.parseCreationOptionsFromJSON(options);
    if (!userExists) {
      challengeContext = context;
      return;
    }

    const attestation = await navigator.credentials.create({ publicKey });
    if (!(attestation instanceof PublicKeyCredential)) {
      alert("Not public key credentials");
      return;
    }

    const { token: t } = await fetch(
      `http://localhost:5223/api/auth/credential/existing`,
      {
        method: "POST",
        body: JSON.stringify({
          attestation,
          challengeContext: context,
        }),
        mode: "cors",
        headers: {
          "Content-Type": "application/json",
        },
      },
    ).then((r) => r.json());
    challengeContext = context;
    token = t;
  }

  async function onSubmitUserName(e: SubmitEvent) {
    if (publicKey == null) {
      alert("Public key is null, how did you get here?");
      return;
    }

    const attestation = await navigator.credentials.create({ publicKey });
    if (!(attestation instanceof PublicKeyCredential)) {
      alert("Not public key credentials");
      return;
    }

    const { token: t } = await fetch(
      `http://localhost:5223/api/auth/credential/new`,
      {
        method: "POST",
        body: JSON.stringify({
          attestation,
          challengeContext,
          userName,
        }),
        mode: "cors",
        headers: {
          "Content-Type": "application/json",
        },
      },
    ).then((r) => r.json());
    token = t;
  }
</script>

{#if phoneNumberVerificationContext == null}
  <form onsubmit={onSubmitPhoneNumber}>
    <label>
      Phone number:
      <input
        type="tel"
        bind:value={phoneNumber}
        pattern="^\+467\d{'{'}8{'}'}$"
        required
        placeholder="+467XXXXXXXX"
      />
    </label>
    <button type="submit">Request verification code</button>
  </form>
{:else if challengeContext == null}
  <form onsubmit={onSubmitVerification}>
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
{:else if token == null}
  <form onsubmit={onSubmitUserName}>
    <label>
      User name:
      <input bind:value={userName} autocomplete="name" required />
    </label>
    <button type="submit">Create user</button>
  </form>
{:else}
  <p>Your token: <code>{token}</code></p>
{/if}
