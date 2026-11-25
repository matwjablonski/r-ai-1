<script setup lang="ts">
const session = useSession();
const busy = ref(false);
async function signOut() {
  busy.value = true;
  try {
    await session.logout();
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <div class="shell">
    <aside v-if="session.user.value" class="sidebar">
      <NuxtLink to="/" class="brand" aria-label="ServiceDesk home">
        <span class="brand-mark">S</span
        ><span>ServiceDesk<small>INTERNAL SUPPORT</small></span>
      </NuxtLink>
      <nav class="nav" aria-label="Main navigation">
        <NuxtLink to="/" class="nav-link">Overview</NuxtLink>
        <NuxtLink to="/tickets" class="nav-link">Tickets</NuxtLink>
        <NuxtLink
          v-if="session.user.value.role === 'Requester'"
          to="/tickets/new"
          class="nav-link"
          >New request</NuxtLink
        >
      </nav>
      <div class="sidebar-foot">
        <div class="avatar" aria-hidden="true">
          {{ session.user.value.displayName.slice(0, 1) }}
        </div>
        <div class="identity">
          <strong>{{ session.user.value.displayName }}</strong
          ><span>{{ session.user.value.role }}</span>
        </div>
        <button class="signout" type="button" :disabled="busy" @click="signOut">
          Sign out
        </button>
      </div>
    </aside>
    <main id="main-content" class="content"><slot /></main>
  </div>
</template>
