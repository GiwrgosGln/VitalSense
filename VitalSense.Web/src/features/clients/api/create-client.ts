import { z } from "zod";
import type { Client } from "@/types/api";
import { api } from "@/lib/api-client";
import type { MutationConfig } from "@/lib/react-query";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export const createClientInputSchema = z.object({
  firstName: z.string().min(4, "Required").max(50, "Max 50 characters"),
  lastName: z.string().min(4, "Required").max(50, "Max 50 characters"),
  email: z.string().email("Invalid email address").optional(),
  phone: z.string().optional(),
  dateOfBirth: z.string().optional(),
  gender: z.string().optional(),
  hasCard: z.boolean().optional(),
  notes: z.string().optional(),
  createdAt: z.string().optional(),
});

export type CreateClientInput = z.infer<typeof createClientInputSchema>;

export const createClient = ({
  data,
}: {
  data: CreateClientInput;
}): Promise<Client> => {
  return api.post("/clients", data);
};

type UseCreateClientOptions = {
  mutationConfig?: MutationConfig<typeof createClient>;
};

export const useCreateClient = ({
  mutationConfig,
}: UseCreateClientOptions = {}) => {
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
    mutationFn: createClient,
  });
};
