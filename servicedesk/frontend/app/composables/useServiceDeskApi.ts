export function useServiceDeskApi() {
  const fetcher = import.meta.server ? useRequestFetch() : $fetch;

  async function get<T>(
    path: string,
    query?: Record<string, string | number | undefined>,
  ) {
    return await fetcher<T>(`/api${path}`, { query });
  }

  async function write<T>(
    path: string,
    method: "POST" | "PATCH",
    body?: Record<string, unknown>,
  ) {
    const { token } = await fetcher<{ token: string }>("/api/auth/csrf");
    return await fetcher<T>(`/api${path}`, {
      method,
      body,
      headers: { "X-CSRF-TOKEN": token },
    });
  }

  return { get, write };
}
