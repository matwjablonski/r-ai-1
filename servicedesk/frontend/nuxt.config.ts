export default defineNuxtConfig({
  modules: ["@nuxt/eslint"],
  css: ["~/assets/css/main.css"],
  typescript: { strict: true },
  devtools: { enabled: false },
  routeRules: {
    "/api/**": {
      proxy: `${process.env.BACKEND_INTERNAL_URL || "http://backend:8080"}/api/**`,
    },
    "/openapi/**": {
      proxy: `${process.env.BACKEND_INTERNAL_URL || "http://backend:8080"}/openapi/**`,
    },
  },
  compatibilityDate: "2026-11-01",
});
