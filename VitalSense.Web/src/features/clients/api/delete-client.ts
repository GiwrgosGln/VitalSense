import { api } from "@/lib/api-client";
import type { MutationConfig } from "@/lib/react-query";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export const deleteClient = ({ clientId }: { clientId: string }) => {
  return api.delete(`/clients/${clientId}`);
};

type UseDeleteClientOptions = {
  mutationConfig?: MutationConfig<typeof deleteClient>;
};

export const useDeleteClient = ({
  mutationConfig,
}: UseDeleteClientOptions = {}) => {
  const queryClient = useQueryClient();

  const { onSuccess, ...restConfig } = mutationConfig || {};

  return useMutation({
    onSuccess: (...args) => {
      queryClient.invalidateQueries({
        // queryKey: getClientsQueryOptions().queryKey,
      });
      onSuccess?.(...args);
    },
    ...restConfig,
    mutationFn: deleteClient,
  });
};
