using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class CreateTaskRequest
{
    public required string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? ClientId { get; set; }
}
