import { queryOptions, useQuery } from "@tanstack/react-query";

import { api } from "@/lib/api-client";
import type { QueryConfig } from "@/lib/react-query";
import type { Client } from "@/types/api";

export const getClients = (): Promise<{ data: Client[] }> => {
  return api.get("/clients");
};

export const getClientsQueryOptions = () => {
  return queryOptions({
    queryKey: ["clients"],
    queryFn: getClients,
  });
};

type UseClientsOptions = {
  queryConfig?: QueryConfig<typeof getClientsQueryOptions>;
};

export const useClients = ({ queryConfig }: UseClientsOptions = {}) => {
  return useQuery({
    ...getClientsQueryOptions(),
    ...queryConfig,
  });
};
