using System.ComponentModel.DataAnnotations;

namespace VitalSense.Application.DTOs;

public class CreateAppointmentRequest
{
    [Required]
    public string Title { get; set; }

    [Required]
    public DateTime Start { get; set; }

    [Required]
    public DateTime End { get; set; }

    [Required]
    public bool AllDay { get; set; }

    [Required]
    public Guid DieticianId { get; set; }

    public Guid ClientId { get; set; }
}