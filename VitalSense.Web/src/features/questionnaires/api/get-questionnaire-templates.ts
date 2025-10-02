import { queryOptions, useQuery } from "@tanstack/react-query";

import { api } from "@/lib/api-client";
import type { QueryConfig } from "@/lib/react-query";
import type { QuestionnaireTemplate } from "../types";

export const getQuestionnaireTemplates = (): Promise<{
  data: QuestionnaireTemplate[];
}> => {
  return api.get("/questionnaire-templates");
};

export const getQuestionnaireTemplatesQueryOptions = () => {
  return queryOptions({
    queryKey: ["questionnaire-templates"],
    queryFn: getQuestionnaireTemplates,
  });
};

type UseQuestionnaireTemplatesOptions = {
  queryConfig?: QueryConfig<typeof getQuestionnaireTemplates>;
};

export const useQuestionnaireTemplates = ({
  queryConfig,
}: UseQuestionnaireTemplatesOptions = {}) => {
  return useQuery({
    ...getQuestionnaireTemplatesQueryOptions(),
    ...queryConfig,
  });
};
