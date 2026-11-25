<script setup lang="ts">
defineProps<{
  label: string;
  tone: "blue" | "amber" | "green" | "slate" | "red";
}>();
</script>
<template>
  <span class="badge" :class="`badge-${tone}`">{{ label }}</span>
</template>
