# Diagnostics and conventions

Use `docker compose ps` to check readiness and `docker compose logs -f migrate backend frontend db` to follow initialization and requests. `docker compose logs --tail=100 backend` shows structured request lines with method, path, status, elapsed milliseconds and `X-Request-ID`; the API returns the same ID as a response header. The health endpoint checks database connectivity. Do not put passwords, CSRF tokens or user data into log messages.

Use browser DevTools Network to inspect the same-origin `/api` requests and timing. For backend profiling, run `dotnet-counters monitor --process-id <pid>` in an optional SDK environment. For database plans, connect to the disposable test database and run `EXPLAIN (ANALYZE, BUFFERS) <query>` on a representative query. Benchmarking is meaningful only with the actual record count and environment recorded. The default seed has 150 tickets.

Keep API authorization in `TicketService`, timestamp calculations in `Rules` with injected `TimeProvider`, EF changes in migrations, and presentation logic in frontend utilities/components. API shape changes should be reflected in `frontend/app/types/api.ts`, tests and the OpenAPI contract. Do not log secrets or add agent instructions to the participant distribution.

Current external integration points are repository hosting/CI in GitLab and optional work tracking in Asana. They are not connected by the running app. Figma designs, MCP tools and SonarQube can be introduced during the workshop without changing local startup. No external endpoint or credential is preconfigured.
