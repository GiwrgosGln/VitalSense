namespace VitalSense.Application.DTOs.Questionnaire;

public class QuestionnaireSubmissionResponse
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateTitle { get; set; } = null!;
    public Guid ClientId { get; set; }
    public DateTime SubmittedAt { get; set; }
    public List<QuestionnaireAnswerResponse> Answers { get; set; } = new();
}

public class QuestionnaireAnswerResponse
{
    public Guid QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string AnswerText { get; set; } = null!;
}