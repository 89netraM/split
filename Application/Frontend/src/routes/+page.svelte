<script lang="ts">
  import { credentialStore } from "$lib/auth/credentialStore";
</script>

<h1>Split</h1>
{#if $credentialStore != null}
  <p><a href="/login">Login</a></p>
{:else}
  <p><a href="/register">Register new device</a></p>
{/if}
