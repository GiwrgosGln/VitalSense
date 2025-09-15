using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("questionnaire_submissions")]
public class QuestionnaireSubmission
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("template_id")]
    public Guid TemplateId { get; set; }

    [Column("dietician_id")]
    public Guid DieticianId { get; set; }

    [Column("client_id")]
    public Guid ClientId { get; set; }

    [Column("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Column("answers")]
    public List<QuestionnaireAnswer> Answers { get; set; } = new();

    [ForeignKey("TemplateId")]
    public QuestionnaireTemplate Template { get; set; } = null!;
}

[Table("questionnaire_answers")]
public class QuestionnaireAnswer
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("submission_id")]
    public Guid SubmissionId { get; set; }

    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [Column("answer_text")]
    public string AnswerText { get; set; } = null!;

    [ForeignKey("SubmissionId")]
    public QuestionnaireSubmission Submission { get; set; } = null!;
}