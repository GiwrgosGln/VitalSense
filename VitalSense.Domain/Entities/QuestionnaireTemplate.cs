using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("questionnaire_templates")]
public class QuestionnaireTemplate
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [Column("questions")]
    public List<QuestionnaireQuestion> Questions { get; set; } = new();

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("dietician_id")]
    public Guid DieticianId { get; set; }
}

[Table("questionnaire_questions")]
public class QuestionnaireQuestion
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("template_id")]
    public Guid TemplateId { get; set; }

    [Column("question_text")]
    public string QuestionText { get; set; } = null!;

    [Column("order")]
    public int Order { get; set; }

    [Column("is_required")]
    public bool IsRequired { get; set; } = false;

    [ForeignKey("TemplateId")]
    public QuestionnaireTemplate Template { get; set; } = null!;
}
