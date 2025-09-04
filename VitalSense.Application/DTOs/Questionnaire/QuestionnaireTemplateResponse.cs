using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs.Questionnaire;

public class QuestionnaireTemplateResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public List<QuestionnaireQuestionResponse> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class QuestionnaireQuestionResponse
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = null!;
    public int Order { get; set; }
    public bool IsRequired { get; set; }
}
