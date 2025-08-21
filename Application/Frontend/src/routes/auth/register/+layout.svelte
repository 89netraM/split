<script lang="ts">
  let { children } = $props();
</script>

<svelte:head><title>Split - Register</title></svelte:head>

<div class="simple-form">
  <h2>Register</h2>
  {@render children()}
</div>
