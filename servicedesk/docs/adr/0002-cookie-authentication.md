# ADR 0002: Local cookie authentication

**Decision:** Hash demo passwords with ASP.NET Core `PasswordHasher`, issue an HttpOnly SameSite=Lax cookie, and validate ASP.NET Core antiforgery tokens supplied in `X-CSRF-TOKEN` for every API write. Nuxt proxies `/api` on the same origin.

**Reason:** Training must work offline without SSO or a paid identity provider. Cookies avoid putting bearer tokens in browser storage. The antiforgery endpoint supports the login request as well as authenticated writes.

**Consequence:** Demo accounts exist only in `Training` seed data. Deployments beyond a trusted local training environment need HTTPS, unique credentials and a separate identity/security review.
