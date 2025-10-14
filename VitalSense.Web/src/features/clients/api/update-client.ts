import { useMutation, useQueryClient } from "@tanstack/react-query";
import { z } from "zod";

import { api } from "@/lib/api-client";
import type { MutationConfig } from "@/lib/react-query";
import type { Client } from "@/types/api";

import { getClientQueryOptions } from "./get-client";

export const updateClientInputSchema = z.object({
  firstName: z.string().min(4, "Required").max(50, "Max 50 characters"),
  lastName: z.string().min(4, "Required").max(50, "Max 50 characters"),
  email: z.string().email("Invalid email address"),
  phone: z.string().optional(),
  dateOfBirth: z.string().optional(),
  gender: z.string().optional(),
  hasCard: z.boolean().optional(),
  notes: z.string().optional(),
  createdAt: z.string().optional(),
});

export type UpdateClientInput = z.infer<typeof updateClientInputSchema>;

export const updateClient = ({
  data,
  clientId,
}: {
  data: UpdateClientInput;
  clientId: string;
}): Promise<Client> => {
  return api.put(`/clients/${clientId}`, data);
};

type UseUpdateClientOptions = {
  mutationConfig?: MutationConfig<typeof updateClient>;
};

export const useUpdateClient = ({
  mutationConfig,
}: UseUpdateClientOptions = {}) => {
  const queryClient = useQueryClient();

  const { onSuccess, ...restConfig } = mutationConfig || {};

  return useMutation({
    onSuccess: (data, ...args) => {
      queryClient.refetchQueries({
        queryKey: getClientQueryOptions(data.id).queryKey,
      });
      onSuccess?.(data, ...args);
    },
    ...restConfig,
    mutationFn: updateClient,
  });
};
