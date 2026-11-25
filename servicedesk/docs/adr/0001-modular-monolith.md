# ADR 0001: One API and one database

**Decision:** Use an ASP.NET Core modular monolith and one PostgreSQL database. Tickets, users, SLA and reporting are code boundaries rather than separately deployed services.

**Reason:** The workshop needs a realistic codebase that runs with one Compose command and can be split into frontend and backend repositories later. Separate services or a broker would add startup and debugging dependencies without a current business need.

**Consequence:** Cross-module operations can use one EF transaction. Reporting currently reads ticket data through the same context, so large datasets require query profiling.
