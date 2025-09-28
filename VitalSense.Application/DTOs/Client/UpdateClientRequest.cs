namespace VitalSense.Application.DTOs;

public class UpdateClientRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool? HasCard { get; set; }
    public string? Notes { get; set; }
}