# Architecture

```text
Browser -> Nuxt (SSR + same-origin /api proxy) -> ASP.NET Core API -> PostgreSQL
```

Compose starts `db`, runs one-shot `migrate`, then starts `backend` and `frontend` after health checks. The API is a modular monolith: endpoint definitions and HTTP concerns in `Program.cs`; ticket application operations and role filtering in `Services/TicketService.cs`; entities and deterministic rules in `Domain/`; EF schema, migrations and synthetic seed in `Data/`. Users, tickets, SLA and reporting share one transactional database. There are no external service dependencies at runtime.

The frontend separates pages, components, API/session composables and typed contracts. It does not duplicate ticket data in a client store. The browser receives only same-origin URLs; Nuxt reaches the backend over the Compose network. Role checks in the UI improve navigation, while the API independently enforces every permission.

Only the two components and Compose files form the distributable application. The workshop's `trainer/` directory, adjacent to `servicedesk/`, belongs to the author and must not be packaged for participants.
