# ServiceDesk frontend

Nuxt 4, Vue 3 and TypeScript UI. Pages use a typed API composable; authentication state is kept in Nuxt `useState`, while server data is fetched through `useAsyncData`. A same-origin `/api/**` proxy sends browser and SSR requests to the backend.

Container start and demo accounts are documented in the root README. For optional native development: `npm ci`, then `BACKEND_INTERNAL_URL=http://localhost:8080 npm run dev`. Run `npm run lint`, `npm run typecheck`, `npm test`, and `npm run build` for local checks. `npm run test:e2e` requires an isolated test stack (`compose.test.yaml`) and Playwright Chromium; the container test service includes it.
