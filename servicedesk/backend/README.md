# ServiceDesk API

ASP.NET Core 10 API with EF Core and PostgreSQL. The application runs in the `backend` Compose service; `migrate` applies EF migrations and the idempotent training seed before the API starts. The `--migrate` argument runs only that initialization step.

The API enforces role visibility in `TicketService`, uses cookie authentication with ASP.NET Core password hashing, and requires `X-CSRF-TOKEN` on writes. Retrieve the token from `GET /api/auth/csrf`. See the root README for demo accounts and container commands.

Local SDK checks: `dotnet restore ServiceDesk.slnx`, `dotnet build ServiceDesk.slnx`, `dotnet test ServiceDesk.slnx`. Integration tests require `ConnectionStrings__ServiceDesk` pointing to a disposable PostgreSQL database; the root `compose.test.yaml` supplies one.

Main source boundaries: `Domain/` contains entities and SLA/status rules, `Services/` contains application operations and authorization, `Data/` contains EF mappings/migrations/seed, and `Program.cs` defines HTTP contracts and middleware. OpenAPI is served at `/openapi/v1.json`.
