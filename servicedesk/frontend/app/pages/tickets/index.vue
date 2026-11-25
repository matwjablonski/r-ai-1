<script setup lang="ts">
import type { Page, Priority, Status, TicketItem } from "~/types/api";
import {
  formatDate,
  priorities,
  priorityLabels,
  priorityTone,
  statuses,
  statusLabels,
  statusTone,
} from "~/utils/presentation";
const api = useServiceDeskApi();
const session = useSession();
const search = ref("");
const statusFilter = ref<Status | "">("");
const priorityFilter = ref<Priority | "">("");
const sort = ref("newest");
const page = ref(1);
const committedSearch = ref("");
watch([statusFilter, priorityFilter, sort], () => {
  page.value = 1;
});
const query = computed(() => ({
  search: committedSearch.value || undefined,
  status: statusFilter.value || undefined,
  priority: priorityFilter.value || undefined,
  sort: sort.value,
  page: page.value,
  pageSize: 20,
}));
const { data, status, error, refresh } = await useAsyncData(
  "ticket-list",
  () => api.get<Page<TicketItem>>("/tickets", query.value),
  { watch: [query] },
);
function applySearch() {
  committedSearch.value = search.value.trim();
  page.value = 1;
}
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <div class="eyebrow">SERVICE REQUESTS</div>
        <h1>Tickets</h1>
        <p>Find a request, review its status, and keep work moving.</p>
      </div>
      <NuxtLink
        v-if="session.user.value?.role === 'Requester'"
        class="button primary"
        to="/tickets/new"
        >+ New request</NuxtLink
      >
    </div>
    <section class="panel filters" aria-label="Ticket filters">
      <form class="search" role="search" @submit.prevent="applySearch">
        <label class="sr-only" for="ticket-search">Search tickets</label
        ><input
          id="ticket-search"
          v-model="search"
          placeholder="Search by title or description"
        ><button class="button secondary" type="submit">Search</button>
      </form>
      <div class="filter-row">
        <label
          >Status<select v-model="statusFilter">
            <option value="">All statuses</option>
            <option v-for="item in statuses" :key="item" :value="item">
              {{ statusLabels[item] }}
            </option>
          </select></label
        ><label
          >Priority<select v-model="priorityFilter">
            <option value="">All priorities</option>
            <option v-for="item in priorities" :key="item" :value="item">
              {{ priorityLabels[item] }}
            </option>
          </select></label
        ><label
          >Sort by<select v-model="sort">
            <option value="newest">Newest first</option>
            <option value="oldest">Oldest first</option>
            <option value="due">Due soonest</option>
            <option value="priority">Highest priority</option>
          </select></label
        >
      </div>
    </section>
    <section class="panel" aria-labelledby="results-title">
      <div class="panel-heading">
        <div>
          <div class="eyebrow">RESULTS</div>
          <h2 id="results-title">{{ data?.total ?? 0 }} tickets</h2>
        </div>
      </div>
      <div v-if="status === 'pending'" class="notice" role="status">
        Loading tickets…
      </div>
      <div v-else-if="error" class="notice error" role="alert">
        Tickets could not be loaded.
        <button class="text-button" type="button" @click="refresh()">
          Retry
        </button>
      </div>
      <div v-else-if="!data?.items.length" class="empty-state">
        No tickets match these filters.
      </div>
      <div v-else class="table-scroll">
        <table>
          <thead>
            <tr>
              <th scope="col">Ticket</th>
              <th scope="col">Requester</th>
              <th scope="col">Assignee</th>
              <th scope="col">Priority</th>
              <th scope="col">Status</th>
              <th scope="col">Due</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ticket in data.items" :key="ticket.id">
              <td>
                <NuxtLink class="ticket-link" :to="`/tickets/${ticket.id}`"
                  ><span class="ticket-id">#{{ ticket.id }}</span
                  >{{ ticket.title }}</NuxtLink
                ><small>{{ ticket.category }}</small>
              </td>
              <td>{{ ticket.requester }}</td>
              <td>{{ ticket.technician || "Unassigned" }}</td>
              <td>
                <StatusBadge
                  :label="priorityLabels[ticket.priority]"
                  :tone="priorityTone(ticket.priority)"
                />
              </td>
              <td>
                <StatusBadge
                  :label="statusLabels[ticket.status]"
                  :tone="statusTone(ticket.status)"
                />
              </td>
              <td>
                <span :class="{ 'danger-text': ticket.overdue }">{{
                  formatDate(ticket.dueAt)
                }}</span
                ><small v-if="ticket.overdue">Overdue</small
                ><small v-else-if="ticket.atRisk">At risk</small>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="data && data.total > data.pageSize" class="pagination">
        <span
          >Page {{ page }} of {{ Math.ceil(data.total / data.pageSize) }}</span
        >
        <div>
          <button
            class="button secondary"
            type="button"
            :disabled="page <= 1"
            @click="page--"
          >
            Previous</button
          ><button
            class="button secondary"
            type="button"
            :disabled="page * data.pageSize >= data.total"
            @click="page++"
          >
            Next
          </button>
        </div>
      </div>
    </section>
  </div>
</template>
