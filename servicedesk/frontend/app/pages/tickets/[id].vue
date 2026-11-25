<script setup lang="ts">
import type { Priority, Status, Technician, TicketDetail } from "~/types/api";
import {
  formatDate,
  priorities,
  priorityLabels,
  priorityTone,
  statusLabels,
  statusTone,
} from "~/utils/presentation";
const route = useRoute();
const api = useServiceDeskApi();
const session = useSession();
const id = Number(route.params.id);
if (!Number.isSafeInteger(id) || id < 1)
  throw createError({ statusCode: 404, statusMessage: "Ticket not found" });
const { data, status, error, refresh } = await useAsyncData(
  `ticket-${id}`,
  () => api.get<TicketDetail>(`/tickets/${id}`),
);
const { data: technicians } = await useAsyncData("technicians", () =>
  session.user.value?.role === "Coordinator"
    ? api.get<Technician[]>("/technicians")
    : Promise.resolve([]),
);
const comment = ref("");
const selectedTechnician = ref("");
const selectedPriority = ref<Priority>("Medium");
const actionError = ref("");
const busy = ref(false);
watch(
  data,
  (detail) => {
    if (detail) selectedPriority.value = detail.ticket.priority;
  },
  { immediate: true },
);
const ticket = computed(() => data.value?.ticket);
const role = computed(() => session.user.value?.role);
const availableStatus = computed<Status[]>(() => {
  if (!ticket.value || role.value === "Requester") return [];
  if (ticket.value.status === "Open" && ticket.value.technician)
    return ["InProgress"];
  if (ticket.value.status === "InProgress") return ["Resolved"];
  if (ticket.value.status === "Resolved" && role.value === "Coordinator")
    return ["InProgress", "Closed"];
  return [];
});
async function action(
  path: string,
  payload: Record<string, unknown>,
  after?: () => void,
) {
  actionError.value = "";
  busy.value = true;
  try {
    await api.write(
      path,
      path.endsWith("/comments") ? "POST" : "PATCH",
      payload,
    );
    after?.();
    await refresh();
  } catch {
    actionError.value =
      "This action could not be completed. The ticket may have changed; refresh and try again.";
  } finally {
    busy.value = false;
  }
}
async function addComment() {
  if (!comment.value.trim()) return;
  await action(`/tickets/${id}/comments`, { body: comment.value }, () => {
    comment.value = "";
  });
}
</script>

<template>
  <div>
    <div v-if="status === 'pending'" class="notice" role="status">
      Loading ticket…
    </div>
    <div v-else-if="error || !data" class="notice error" role="alert">
      Ticket unavailable or access denied.
      <NuxtLink to="/tickets">Back to tickets</NuxtLink>
    </div>
    <div v-else>
      <div class="page-header">
        <div>
          <div class="eyebrow">SERVICE REQUESTS / #{{ id }}</div>
          <h1>{{ ticket?.title }}</h1>
          <p>
            Created {{ formatDate(ticket!.createdAt) }} · {{ ticket?.category }}
          </p>
        </div>
        <NuxtLink class="text-link" to="/tickets">← Back to tickets</NuxtLink>
      </div>
      <div class="detail-grid">
        <div class="detail-main">
          <section class="panel">
            <div class="panel-heading">
              <h2>About this request</h2>
              <div class="badge-row">
                <StatusBadge
                  :label="priorityLabels[ticket!.priority]"
                  :tone="priorityTone(ticket!.priority)"
                /><StatusBadge
                  :label="statusLabels[ticket!.status]"
                  :tone="statusTone(ticket!.status)"
                />
              </div>
            </div>
            <p class="description">{{ data.description }}</p>
          </section>
          <section class="panel">
            <div class="panel-heading">
              <h2>Conversation</h2>
              <span class="count">{{ data.comments.length }} comments</span>
            </div>
            <div v-if="!data.comments.length" class="empty-state">
              No comments yet.
            </div>
            <div v-for="item in data.comments" :key="item.id" class="activity">
              <div class="avatar small" aria-hidden="true">
                {{ item.author.slice(0, 1) }}
              </div>
              <div>
                <div class="activity-meta">
                  <strong>{{ item.author }}</strong
                  ><time :datetime="item.createdAt">{{
                    formatDate(item.createdAt)
                  }}</time>
                </div>
                <p>{{ item.body }}</p>
              </div>
            </div>
            <form class="comment-form" @submit.prevent="addComment">
              <label for="comment">Add a comment</label
              ><textarea
                id="comment"
                v-model="comment"
                rows="3"
                maxlength="4000"
                required
                placeholder="Add context for the team…"
              /><button
                class="button primary"
                type="submit"
                :disabled="busy || !comment.trim()"
              >
                Post comment
              </button>
            </form>
          </section>
          <section class="panel">
            <div class="panel-heading"><h2>History</h2></div>
            <ol class="timeline">
              <li v-for="entry in data.history" :key="entry.id">
                <strong>{{ entry.detail }}</strong
                ><span
                  >{{ entry.actor }} ·
                  <time :datetime="entry.createdAt">{{
                    formatDate(entry.createdAt)
                  }}</time></span
                >
              </li>
            </ol>
          </section>
        </div>
        <aside class="detail-side">
          <section class="panel">
            <h2>Ticket details</h2>
            <dl class="facts">
              <div>
                <dt>Requester</dt>
                <dd>{{ ticket?.requester }}</dd>
              </div>
              <div>
                <dt>Technician</dt>
                <dd>{{ ticket?.technician || "Unassigned" }}</dd>
              </div>
              <div>
                <dt>Priority</dt>
                <dd>{{ priorityLabels[ticket!.priority] }}</dd>
              </div>
              <div>
                <dt>SLA deadline</dt>
                <dd>
                  {{ formatDate(ticket!.dueAt)
                  }}<span v-if="ticket?.overdue" class="danger-text">
                    · Overdue</span
                  ><span v-else-if="ticket?.atRisk" class="warning-text">
                    · At risk</span
                  >
                </dd>
              </div>
            </dl>
          </section>
          <section v-if="role !== 'Requester'" class="panel actions">
            <h2>Manage ticket</h2>
            <p v-if="actionError" class="form-error" role="alert">
              {{ actionError }}
            </p>
            <div v-if="role === 'Coordinator'" class="action-group">
              <label for="technician">Assign technician</label
              ><select id="technician" v-model="selectedTechnician">
                <option value="" disabled>Select a technician</option>
                <option
                  v-for="person in technicians"
                  :key="person.id"
                  :value="person.id"
                >
                  {{ person.displayName }}
                </option></select
              ><button
                class="button secondary"
                type="button"
                :disabled="busy || !selectedTechnician"
                @click="
                  action(`/tickets/${id}/assignment`, {
                    technicianId: selectedTechnician,
                  })
                "
              >
                Assign
              </button>
            </div>
            <div v-if="role === 'Coordinator'" class="action-group">
              <label for="priority">Priority</label
              ><select id="priority" v-model="selectedPriority">
                <option v-for="item in priorities" :key="item" :value="item">
                  {{ priorityLabels[item] }}
                </option></select
              ><button
                class="button secondary"
                type="button"
                :disabled="busy || selectedPriority === ticket?.priority"
                @click="
                  action(`/tickets/${id}/priority`, {
                    priority: selectedPriority,
                  })
                "
              >
                Update priority
              </button>
            </div>
            <div v-if="availableStatus.length" class="action-group">
              <span class="field-label">Move to</span
              ><button
                v-for="next in availableStatus"
                :key="next"
                type="button"
                class="button secondary"
                :disabled="busy"
                @click="action(`/tickets/${id}/status`, { status: next })"
              >
                {{ statusLabels[next] }}
              </button>
            </div>
            <p
              v-if="!availableStatus.length && role === 'Technician'"
              class="helper"
            >
              No status change is available for this ticket.
            </p>
          </section>
        </aside>
      </div>
    </div>
  </div>
</template>
