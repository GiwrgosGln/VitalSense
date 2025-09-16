using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Application.DTOs;

public class AppointmentWithClientInfoResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool AllDay { get; set; }
    public Guid DieticianId { get; set; }
    public Guid ClientId { get; set; }

    // Client Info
    public string ClientFirstName { get; set; }
    public string ClientLastName { get; set; }
    public string ClientEmail { get; set; }
    public string ClientPhone { get; set; }
}