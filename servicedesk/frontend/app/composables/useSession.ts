import type { User } from "~/types/api";

export function useSession() {
  const user = useState<User | null>("session-user", () => null);
  const api = useServiceDeskApi();

  async function load() {
    try {
      user.value = await api.get<User>("/auth/me");
    } catch {
      user.value = null;
    }
    return user.value;
  }

  async function logout() {
    await api.write("/auth/logout", "POST");
    user.value = null;
    await navigateTo("/login");
  }

  return { user, load, logout };
}
