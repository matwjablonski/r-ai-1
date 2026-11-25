export default defineNuxtRouteMiddleware(async (to) => {
  if (to.path === "/login") return;
  const session = useSession();
  if (session.user.value || (await session.load())) return;
  return navigateTo("/login");
});
