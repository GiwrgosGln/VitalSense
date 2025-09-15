using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs.Questionnaire;

public class QuestionnaireSubmissionRequest
{
    [Required]
    public Guid TemplateId { get; set; }
    
    [Required]
    public Guid ClientId { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one answer is required")]
    public List<QuestionnaireAnswerRequest> Answers { get; set; } = new();
}

public class QuestionnaireAnswerRequest
{
    [Required]
    public Guid QuestionId { get; set; }
    
    [Required]
    public string AnswerText { get; set; } = null!;
}