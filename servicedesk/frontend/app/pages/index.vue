<script setup lang="ts">
import type { Dashboard, Page, TicketItem } from "~/types/api";
import {
  formatDate,
  statusLabels,
  statusTone,
  priorityLabels,
  priorityTone,
} from "~/utils/presentation";
const api = useServiceDeskApi();
const session = useSession();
const {
  data: dashboard,
  status: dashboardStatus,
  error: dashboardError,
  refresh: refreshDashboard,
} = await useAsyncData("dashboard", () => api.get<Dashboard>("/dashboard"));
const { data: recent, status: recentStatus } = await useAsyncData(
  "recent-tickets",
  () => api.get<Page<TicketItem>>("/tickets", { pageSize: 6 }),
);
const heading = computed(() =>
  session.user.value?.role === "Requester"
    ? "Your support at a glance"
    : "Support operations at a glance",
);
</script>

<template>
  <div>
    <div class="page-header">
      <div>
        <div class="eyebrow">
          OVERVIEW / {{ session.user.value?.role.toUpperCase() }}
        </div>
        <h1>{{ heading }}</h1>
        <p>Follow the work that matters, from request to resolution.</p>
      </div>
      <NuxtLink
        v-if="session.user.value?.role === 'Requester'"
        class="button primary"
        to="/tickets/new"
        >+ New request</NuxtLink
      >
    </div>
    <div v-if="dashboardStatus === 'pending'" class="notice" role="status">
      Loading dashboard…
    </div>
    <div v-else-if="dashboardError" class="notice error" role="alert">
      Dashboard is unavailable.
      <button type="button" class="text-button" @click="refreshDashboard()">
        Retry
      </button>
    </div>
    <section v-else-if="dashboard" class="metrics" aria-label="Ticket summary">
      <MetricCard label="Open" :value="dashboard.open" note="Awaiting action" />
      <MetricCard
        label="In progress"
        :value="dashboard.inProgress"
        note="Being investigated"
      />
      <MetricCard
        label="At risk"
        :value="dashboard.atRisk"
        note="Approaching SLA"
        tone="risk"
      />
      <MetricCard
        label="Overdue"
        :value="dashboard.overdue"
        note="Past SLA deadline"
        tone="danger"
      />
      <MetricCard
        label="Resolved today"
        :value="dashboard.resolvedToday"
        note="Closed the loop"
      />
    </section>
    <section class="panel" aria-labelledby="recent-title">
      <div class="panel-heading">
        <div>
          <div class="eyebrow">LATEST ACTIVITY</div>
          <h2 id="recent-title">Recent tickets</h2>
        </div>
        <NuxtLink class="text-link" to="/tickets">View all tickets →</NuxtLink>
      </div>
      <div v-if="recentStatus === 'pending'" class="notice" role="status">
        Loading tickets…
      </div>
      <div v-else-if="!recent?.items.length" class="empty-state">
        No tickets yet. New requests will appear here.
      </div>
      <div v-else class="table-scroll">
        <table>
          <thead>
            <tr>
              <th scope="col">Ticket</th>
              <th scope="col">Priority</th>
              <th scope="col">Status</th>
              <th scope="col">Due</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="ticket in recent.items" :key="ticket.id">
              <td>
                <NuxtLink :to="`/tickets/${ticket.id}`" class="ticket-link"
                  ><span class="ticket-id">#{{ ticket.id }}</span
                  >{{ ticket.title }}</NuxtLink
                ><small>{{ ticket.category }}</small>
              </td>
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
              <td>{{ formatDate(ticket.dueAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>
