export type QuestionnaireTemplate = {
  id: string;
  title: string;
  description: string;
  questions: QuestionnaireTemplateQuestion[];
  createdAt: string;
  updatedAt: string;
};

export type QuestionnaireTemplateQuestion = {
  id: string;
  questionText: string;
  order: number;
  isRequired: boolean;
};
