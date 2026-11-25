<script setup lang="ts">
import type { Category } from "~/types/api";
const api = useServiceDeskApi();
const session = useSession();
if (session.user.value?.role !== "Requester") await navigateTo("/tickets");
const { data: categories, error: categoriesError } = await useAsyncData(
  "categories",
  () => api.get<Category[]>("/categories"),
);
const title = ref("");
const description = ref("");
const categoryId = ref("");
const error = ref("");
const busy = ref(false);
async function submit() {
  error.value = "";
  busy.value = true;
  try {
    const result = await api.write<{ id: number }>("/tickets", "POST", {
      title: title.value,
      description: description.value,
      categoryId: Number(categoryId.value),
    });
    await navigateTo(`/tickets/${result.id}`);
  } catch {
    error.value =
      "The request could not be created. Check the fields and try again.";
  } finally {
    busy.value = false;
  }
}
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <div class="eyebrow">SERVICE REQUESTS / NEW</div>
        <h1>New request</h1>
        <p>Tell the support team what happened and what you expected.</p>
      </div>
      <NuxtLink class="text-link" to="/tickets">← Back to tickets</NuxtLink>
    </div>
    <section class="panel form-panel" aria-labelledby="new-title">
      <h2 id="new-title">Request details</h2>
      <p>Required fields are marked with *.</p>
      <p v-if="categoriesError" class="form-error" role="alert">
        Categories are unavailable. Reload the page to try again.
      </p>
      <form @submit.prevent="submit">
        <label for="title">Title *</label
        ><input
          id="title"
          v-model="title"
          maxlength="180"
          required
          placeholder="A short summary of the issue"
        ><label for="category">Category *</label
        ><select id="category" v-model="categoryId" required>
          <option value="" disabled>Select a category</option>
          <option
            v-for="category in categories"
            :key="category.id"
            :value="String(category.id)"
          >
            {{ category.name }}
          </option></select
        ><label for="description">Description *</label
        ><textarea
          id="description"
          v-model="description"
          maxlength="6000"
          rows="7"
          required
          placeholder="What happened? What did you try?"
        />
        <p v-if="error" class="form-error" role="alert">{{ error }}</p>
        <div class="form-actions">
          <NuxtLink class="button secondary" to="/tickets">Cancel</NuxtLink
          ><button
            class="button primary"
            type="submit"
            :disabled="busy || !categories?.length"
          >
            {{ busy ? "Creating…" : "Create request" }}
          </button>
        </div>
      </form>
    </section>
  </div>
</template>
