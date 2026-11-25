<script setup lang="ts">
defineProps<{
  label: string;
  value: number | string;
  note: string;
  tone?: string;
}>();
</script>
<template>
  <div class="metric" :class="tone">
    <span class="metric-label">{{ label }}</span
    ><strong>{{ value }}</strong
    ><small>{{ note }}</small>
  </div>
</template>
