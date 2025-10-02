import { z } from "zod";
import type { QuestionnaireTemplate } from "../types";
import { api } from "@/lib/api-client";
import type { MutationConfig } from "@/lib/react-query";
import { useMutation, useQueryClient } from "@tanstack/react-query";

const createQuestionnaireQuestionSchema = z.object({
  questionText: z
    .string()
    .min(5, "Question text must be at least 5 characters long.")
    .max(500, "Question text cannot exceed 500 characters."),
  order: z.number().int(),
  isRequired: z.boolean(),
});

export const createQuestionnaireTemplateSchema = z.object({
  title: z
    .string()
    .min(3, "Title must be at least 3 characters long.")
    .max(100, "Title cannot exceed 100 characters."),
  description: z
    .string()
    .max(500, "Description cannot exceed 500 characters.")
    .optional(),
  questions: z
    .array(createQuestionnaireQuestionSchema)
    .min(1, "At least one question is required."),
});

export type CreateQuestionnaireTemplateInput = z.infer<
  typeof createQuestionnaireTemplateSchema
>;

export const createQuestionnaireTemplate = ({
  data,
}: {
  data: CreateQuestionnaireTemplateInput;
}): Promise<QuestionnaireTemplate> => {
  return api.post("/questionnaire-templates", data);
};

type UseCreateQuestionnaireTemplateOptions = {
  mutationConfig?: MutationConfig<typeof createQuestionnaireTemplate>;
};

export const UseCreateQuestionnaireTemplate = ({
  mutationConfig,
}: UseCreateQuestionnaireTemplateOptions = {}) => {
  const queryClient = useQueryClient();

  const { onSuccess, ...restConfig } = mutationConfig || {};

  return useMutation({
    onSuccess: (...args) => {
      queryClient.invalidateQueries({
        //queryKey: getQuestionnaireTemplatesQueryOptions().queryKey,
      });
      onSuccess?.(...args);
    },
    ...restConfig,
    mutationFn: createQuestionnaireTemplate,
  });
};
