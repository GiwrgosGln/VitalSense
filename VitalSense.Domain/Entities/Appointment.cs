using System.ComponentModel.DataAnnotations.Schema;

namespace VitalSense.Domain.Entities;

[Table("appointments")]
public class Appointment
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("start")]
    public DateTime Start { get; set; }

    [Column("end")]
    public DateTime End { get; set; }

    [Column("all_day")]
    public bool AllDay { get; set; }

    [Column("dietician_id")]
    public Guid DieticianId { get; set; }

    [Column("client_id")]
    public Guid ClientId { get; set; }
}