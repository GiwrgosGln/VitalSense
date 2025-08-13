namespace VitalSense.Application.DTOs;

public class DashboardMetricsResponse
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public int NewClientsThisMonth { get; set; }
    public int NewClientsLastMonth { get; set; }
    public double NewClientsChangePercent { get; set; }
    public string NewClientsChangePercentFormatted { get; set; } = "0%";
}
