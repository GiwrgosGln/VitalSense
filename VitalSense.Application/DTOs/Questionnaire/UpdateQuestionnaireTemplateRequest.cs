using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs.Questionnaire;

public class UpdateQuestionnaireTemplateRequest
{
    [Required]
    [MinLength(3, ErrorMessage = "Title must be at least 3 characters long.")]
    [MaxLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
    public required string Title { get; set; }

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? Description { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one question is required.")]
    public required List<UpdateQuestionnaireQuestionRequest> Questions { get; set; }
}

public class UpdateQuestionnaireQuestionRequest
{
    public Guid? Id { get; set; }

    [Required]
    [MinLength(5, ErrorMessage = "Question text must be at least 5 characters long.")]
    [MaxLength(500, ErrorMessage = "Question text cannot exceed 500 characters.")]
    public required string QuestionText { get; set; }

    [Required]
    public required int Order { get; set; }

    public bool IsRequired { get; set; } = false;
}
