export type Role = "Requester" | "Technician" | "Coordinator";
export type Status = "Open" | "InProgress" | "Resolved" | "Closed";
export type Priority = "Low" | "Medium" | "High" | "Critical";
export interface User {
  id: string;
  email?: string;
  displayName: string;
  role: Role;
}
export interface TicketItem {
  id: number;
  title: string;
  category: string;
  priority: Priority;
  status: Status;
  requester: string;
  technician: string | null;
  createdAt: string;
  dueAt: string;
  atRisk: boolean;
  overdue: boolean;
}
export interface CommentItem {
  id: number;
  author: string;
  body: string;
  createdAt: string;
}
export interface HistoryItem {
  id: number;
  actor: string;
  action: string;
  detail: string;
  createdAt: string;
}
export interface TicketDetail {
  ticket: TicketItem;
  description: string;
  updatedAt: string;
  resolvedAt: string | null;
  comments: CommentItem[];
  history: HistoryItem[];
}
export interface Page<T> {
  items: T[];
  total: number;
  pageNumber: number;
  pageSize: number;
}
export interface Dashboard {
  open: number;
  inProgress: number;
  atRisk: number;
  overdue: number;
  resolvedToday: number;
}
export interface Category {
  id: number;
  name: string;
}
export interface Technician {
  id: string;
  displayName: string;
}
