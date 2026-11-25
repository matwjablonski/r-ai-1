# Testing and local development

The basic application needs only Docker Compose. For container-only checks, from the ServiceDesk root run:

```sh
docker compose -f compose.test.yaml run --build --rm backend-tests
docker compose -f compose.test.yaml run --build --rm frontend-checks
docker compose -f compose.test.yaml up --build --abort-on-container-exit --exit-code-from frontend-e2e frontend-e2e
docker compose -f compose.test.yaml down
```

The test stack has a separate Compose project and PostgreSQL tmpfs database. It does not connect to the participant database. The browser test creates a uniquely named ticket and verifies requester isolation, search, assignment and status changes. Frontend checks cover lint, typecheck, Vitest and production build. Backend checks include xUnit rules and API tests against PostgreSQL.

Optional native commands: start only `docker compose -f compose.test.yaml up -d db`, then set `ConnectionStrings__ServiceDesk='Host=127.0.0.1;Port=15432;Database=servicedesk_test;Username=servicedesk;Password=test-only-password'` and run `dotnet test backend/ServiceDesk.slnx`. In `frontend`, run `npm ci`, `npm run lint`, `npm run typecheck`, `npm test`, and `npm run build`. For native browser tests, start the isolated web stack with `docker compose -f compose.test.yaml up --build -d frontend`, then `npm run test:e2e` from `frontend`. Install Playwright Chromium once with `npx playwright install chromium` if it is not present.

Do not point integration tests at the participant database. The test factory applies migrations and seed and tests insert records.
