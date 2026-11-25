import type { Priority, Status } from "~/types/api";
type Tone = "blue" | "amber" | "green" | "slate" | "red";

export const statuses: Status[] = ["Open", "InProgress", "Resolved", "Closed"];
export const priorities: Priority[] = ["Low", "Medium", "High", "Critical"];
export const statusLabels: Record<Status, string> = {
  Open: "Open",
  InProgress: "In progress",
  Resolved: "Resolved",
  Closed: "Closed",
};
export const priorityLabels: Record<Priority, string> = {
  Low: "Low",
  Medium: "Medium",
  High: "High",
  Critical: "Critical",
};
export function formatDate(value: string) {
  return new Intl.DateTimeFormat("en-GB", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
export function statusTone(status: Status): Tone {
  const tones: Record<Status, Tone> = {
    Open: "blue",
    InProgress: "amber",
    Resolved: "green",
    Closed: "slate",
  };
  return tones[status];
}
export function priorityTone(priority: Priority): Tone {
  const tones: Record<Priority, Tone> = {
    Low: "slate",
    Medium: "blue",
    High: "amber",
    Critical: "red",
  };
  return tones[priority];
}
