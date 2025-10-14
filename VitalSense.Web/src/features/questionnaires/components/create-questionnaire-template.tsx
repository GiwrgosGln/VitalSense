import { zodResolver } from "@hookform/resolvers/zod";
import { useFieldArray, useForm } from "react-hook-form";
import { toast } from "sonner";
import { Plus, Trash2 } from "lucide-react";
import {
  createQuestionnaireTemplateSchema,
  type CreateQuestionnaireTemplateInput,
  UseCreateQuestionnaireTemplate,
} from "../api/create-questionnaire-template";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

export const CreateQuestionnaireTemplate = () => {
  const createQuestionnaireTemplateMutation = UseCreateQuestionnaireTemplate({
    mutationConfig: {
      onSuccess: () => {
        toast.success("Questionnaire Template has been created successfully.");
        form.reset();
      },
      onError: (error) => {
        toast.error("Failed to create questionnaire template.", {
          description: error.message,
        });
      },
    },
  });

  const form = useForm<CreateQuestionnaireTemplateInput>({
    resolver: zodResolver(createQuestionnaireTemplateSchema),
    defaultValues: {
      title: "",
      description: "",
      questions: [
        {
          questionText: "",
          order: 1,
          isRequired: false,
        },
      ],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control: form.control,
    name: "questions",
  });

  const onSubmit = (data: CreateQuestionnaireTemplateInput) => {
    createQuestionnaireTemplateMutation.mutate({ data });
  };

  const addQuestion = () => {
    append({
      questionText: "",
      order: fields.length + 1,
      isRequired: false,
    });
  };

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div>
        <h2 className="text-2xl font-bold tracking-tight">
          Create Questionnaire Template
        </h2>
        <p className="text-muted-foreground">
          Create a new questionnaire template with custom questions.
        </p>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Template Information</CardTitle>
              <CardDescription>
                Basic information about the questionnaire template
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <FormField
                control={form.control}
                name="title"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Title</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Enter template title"
                        {...field}
                        disabled={createQuestionnaireTemplateMutation.isPending}
                      />
                    </FormControl>
                    <FormDescription>
                      A descriptive title for your questionnaire template
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Description</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Enter template description (optional)"
                        className="resize-none"
                        rows={3}
                        {...field}
                        disabled={createQuestionnaireTemplateMutation.isPending}
                      />
                    </FormControl>
                    <FormDescription>
                      Optional description to provide more context
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Questions</CardTitle>
              <CardDescription>
                Add questions to your questionnaire template
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              {fields.map((field, index) => (
                <Card key={field.id} className="relative">
                  <CardContent className="space-y-4 pt-6">
                    <div className="flex items-start justify-between">
                      <h4 className="font-semibold">Question {index + 1}</h4>
                      {fields.length > 1 && (
                        <Button
                          type="button"
                          variant="ghost"
                          size="sm"
                          onClick={() => remove(index)}
                          disabled={
                            createQuestionnaireTemplateMutation.isPending
                          }
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      )}
                    </div>

                    <FormField
                      control={form.control}
                      name={`questions.${index}.questionText`}
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Question Text</FormLabel>
                          <FormControl>
                            <Textarea
                              placeholder="Enter your question"
                              className="resize-none"
                              rows={2}
                              {...field}
                              disabled={
                                createQuestionnaireTemplateMutation.isPending
                              }
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="flex items-center gap-4">
                      <FormField
                        control={form.control}
                        name={`questions.${index}.order`}
                        render={({ field }) => (
                          <FormItem className="flex-1">
                            <FormLabel>Order</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                {...field}
                                onChange={(e) =>
                                  field.onChange(parseInt(e.target.value, 10))
                                }
                                disabled={
                                  createQuestionnaireTemplateMutation.isPending
                                }
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />

                      <FormField
                        control={form.control}
                        name={`questions.${index}.isRequired`}
                        render={({ field }) => (
                          <FormItem className="flex flex-row items-center space-x-2 space-y-0 pt-8">
                            <FormControl>
                              <Checkbox
                                checked={field.value}
                                onCheckedChange={field.onChange}
                                disabled={
                                  createQuestionnaireTemplateMutation.isPending
                                }
                              />
                            </FormControl>
                            <FormLabel className="font-normal">
                              Required
                            </FormLabel>
                          </FormItem>
                        )}
                      />
                    </div>
                  </CardContent>
                </Card>
              ))}

              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={addQuestion}
                disabled={createQuestionnaireTemplateMutation.isPending}
                className="w-full"
              >
                <Plus className="mr-2 h-4 w-4" />
                Add Question
              </Button>
            </CardContent>
          </Card>

          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => form.reset()}
              disabled={createQuestionnaireTemplateMutation.isPending}
            >
              Cancel
            </Button>
            <Button
              type="submit"
              disabled={createQuestionnaireTemplateMutation.isPending}
            >
              {createQuestionnaireTemplateMutation.isPending
                ? "Creating..."
                : "Create Template"}
            </Button>
          </div>
        </form>
      </Form>
    </div>
  );
};
