using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Application.DTOs;

public class AppointmentResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool AllDay { get; set; }
    public Guid DieticianId { get; set; }
    public Guid ClientId { get; set; }
}