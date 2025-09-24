import { useQuery, queryOptions } from "@tanstack/react-query";

import { api } from "@/lib/api-client";
import type { QueryConfig } from "@/lib/react-query";
import type { Client } from "@/types/api";

export const searchClient = ({
  q,
  limit = 20,
}: {
  q: string;
  limit?: number;
}): Promise<{ data: Client[] }> => {
  return api.get("/clients/search", {
    params: { q, limit },
  });
};

export const getSearchClientQueryOptions = (q: string, limit: number = 20) => {
  return queryOptions({
    queryKey: ["clients", "search", q, limit],
    queryFn: () => searchClient({ q, limit }),
  });
};

type UseSearchClientOptions = {
  q: string;
  limit?: number;
  queryConfig?: QueryConfig<typeof getSearchClientQueryOptions>;
};

export const useSearchClient = ({
  q,
  limit = 20,
  queryConfig,
}: UseSearchClientOptions) => {
  return useQuery({
    ...getSearchClientQueryOptions(q, limit),
    ...queryConfig,
  });
};
