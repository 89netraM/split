<script lang="ts">
  let phoneNumber = $state("");

  async function onsubmit(e: SubmitEvent) {
    const { context, options } = await fetch(`http://localhost:5223/api/auth/assertion?phoneNumber=${phoneNumber}`).then((r) => r.json());
    const assertion = await navigator.credentials.get({ publicKey: PublicKeyCredential.parseRequestOptionsFromJSON(options) });
    if (!(assertion instanceof PublicKeyCredential)) {
      alert("Not public key credentials");
      return;
    }
    const response = await fetch("http://localhost:5223/api/auth/assertion", {
      method: "POST",
      body: JSON.stringify({ context, assertion }),
      mode: "cors",
      headers: {
        "Content-Type": "application/json",
      },
    })
    console.log(await response.json());
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
  <button type="submit">Login with a Passkey</button>
</form>
