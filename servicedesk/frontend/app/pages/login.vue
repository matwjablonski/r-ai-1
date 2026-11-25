<script setup lang="ts">
import type { User } from "~/types/api";
const api = useServiceDeskApi();
const session = useSession();
const email = ref("");
const password = ref("");
const error = ref("");
const busy = ref(false);
async function submit() {
  error.value = "";
  busy.value = true;
  try {
    session.user.value = await api.write<User>("/auth/login", "POST", {
      email: email.value,
      password: password.value,
    });
    await navigateTo("/");
  } catch {
    error.value = "Check your email and password, then try again.";
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-intro">
      <div class="eyebrow">INTERNAL SUPPORT</div>
      <h1>Make every request count.</h1>
      <p>Track technical issues from first report to verified resolution.</p>
      <div class="login-art" aria-hidden="true">
        <span>01 / REPORT</span><span>02 / INVESTIGATE</span
        ><span>03 / RESOLVE</span>
      </div>
    </div>
    <section class="login-panel" aria-labelledby="login-title">
      <div class="brand">
        <span class="brand-mark">S</span><span>ServiceDesk</span>
      </div>
      <h2 id="login-title">Welcome back</h2>
      <p>Sign in to your training account.</p>
      <form @submit.prevent="submit">
        <label for="email">Email</label
        ><input
          id="email"
          v-model="email"
          autocomplete="username"
          type="email"
          required
        >
        <label for="password">Password</label
        ><input
          id="password"
          v-model="password"
          autocomplete="current-password"
          type="password"
          required
        >
        <p v-if="error" class="form-error" role="alert">{{ error }}</p>
        <button class="button primary full" type="submit" :disabled="busy">
          {{ busy ? "Signing in…" : "Sign in" }}
        </button>
      </form>
      <p class="login-hint">Demo accounts are listed in the project README.</p>
    </section>
  </div>
</template>
