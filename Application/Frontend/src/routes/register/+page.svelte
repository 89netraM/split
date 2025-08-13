<script lang="ts">
  let phoneNumber = $state("");

  async function onsubmit(e: SubmitEvent) {
    const { context, options } = await fetch(`http://localhost:5223/api/auth/credential?phoneNumber=${phoneNumber}`).then((r) => r.json());
    const attestation = await navigator.credentials.create({ publicKey: PublicKeyCredential.parseCreationOptionsFromJSON(options) });
    if (!(attestation instanceof PublicKeyCredential)) {
      alert("Not public key credentials");
      return;
    }
    await fetch("http://localhost:5223/api/auth/credential", {
      method: "POST",
      body: JSON.stringify({ context, attestation }),
      mode: "cors",
      headers: {
        "Content-Type": "application/json",
      },
    })
  }
</script>

<form {onsubmit}>
  <label>
    <input
      type="tel"
      bind:value={phoneNumber}
      pattern="\+46 7\d-\d\d\d \d\d \d\d"
      required
      placeholder="+46 7X-XXX XX XX"
    />
  </label>
  <button type="submit">Register with a Passkey</button>
</form>
