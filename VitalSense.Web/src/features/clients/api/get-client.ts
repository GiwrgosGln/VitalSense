import { useQuery, queryOptions } from "@tanstack/react-query";

import { api } from "@/lib/api-client";
import type { QueryConfig } from "@/lib/react-query";
import type { Client } from "@/types/api";

export const getClient = ({
  clientId,
}: {
  clientId: string;
}): Promise<{ data: Client }> => {
  return api.get(`/clients/${clientId}`);
};

export const getClientQueryOptions = (clientId: string) => {
  return queryOptions({
    queryKey: ["clients", clientId],
    queryFn: () => getClient({ clientId }),
  });
};

type UseClientOptions = {
  clientId: string;
  queryConfig?: QueryConfig<typeof getClientQueryOptions>;
};

export const useClient = ({ clientId, queryConfig }: UseClientOptions) => {
  return useQuery({
    ...getClientQueryOptions(clientId),
    ...queryConfig,
  });
};
