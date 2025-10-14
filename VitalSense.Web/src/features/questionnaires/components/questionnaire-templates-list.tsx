import { useQuestionnaireTemplates } from "../api/get-questionnaire-templates";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { FileText, Edit, Trash2, Eye } from "lucide-react";

export const QuestionnaireTemplatesList = () => {
  const questionnaireTemplatesQuery = useQuestionnaireTemplates();

  if (questionnaireTemplatesQuery.isLoading) {
    return (
      <div className="flex h-48 w-full items-center justify-center">
        <p>Loading...</p>
      </div>
    );
  }

  const questionnaireTemplates = questionnaireTemplatesQuery.data?.data;

  if (!questionnaireTemplates || questionnaireTemplates.length === 0) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-16">
          <FileText className="h-12 w-12 text-muted-foreground" />
          <h3 className="mt-4 text-lg font-semibold">
            No questionnaire templates
          </h3>
          <p className="mt-2 text-center text-sm text-muted-foreground">
            Get started by creating your first questionnaire template.
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {questionnaireTemplates.map((template) => (
          <Card key={template.id} className="flex flex-col">
            <CardHeader>
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="line-clamp-1">
                    {template.title}
                  </CardTitle>
                  <CardDescription className="mt-1.5 line-clamp-2">
                    {template.description || "No description provided"}
                  </CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent className="flex flex-1 flex-col justify-between">
              <div className="space-y-3">
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <FileText className="h-4 w-4" />
                  <span>
                    {template.questions?.length || 0}{" "}
                    {template.questions?.length === 1
                      ? "question"
                      : "questions"}
                  </span>
                </div>

                {template.questions && template.questions.length > 0 && (
                  <div className="space-y-1">
                    <p className="text-xs font-medium text-muted-foreground">
                      Sample questions:
                    </p>
                    <ul className="space-y-1">
                      {template.questions.slice(0, 2).map((question, idx) => (
                        <li
                          key={idx}
                          className="line-clamp-1 text-xs text-muted-foreground"
                        >
                          {idx + 1}. {question.questionText}
                        </li>
                      ))}
                      {template.questions.length > 2 && (
                        <li className="text-xs text-muted-foreground">
                          +{template.questions.length - 2} more
                        </li>
                      )}
                    </ul>
                  </div>
                )}
              </div>

              <div className="mt-4 flex items-center gap-2">
                <Button variant="outline" size="sm" className="flex-1">
                  <Eye className="mr-2 h-4 w-4" />
                  View
                </Button>
                <Button variant="outline" size="sm">
                  <Edit className="h-4 w-4" />
                </Button>
                <Button variant="outline" size="sm">
                  <Trash2 className="h-4 w-4 text-destructive" />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
};
