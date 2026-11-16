import { api, type Address } from "../api";
import { useAsync } from "../useAsync";
export function useAddress(addressId: string | undefined) {
  return useAsync<Address | null>(
    async () => (await api.searchAddresses()).find((a) => a.id === addressId) ?? null,
    [addressId],
  );
}
