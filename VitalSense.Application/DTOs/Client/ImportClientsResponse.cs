namespace VitalSense.Application.DTOs;

public class ImportClientsResponse
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public List<ClientResponse> ImportedClients { get; set; } = new List<ClientResponse>();
}