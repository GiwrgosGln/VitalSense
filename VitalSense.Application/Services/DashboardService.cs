using VitalSense.Infrastructure.Data;
using VitalSense.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using VitalSense.Application.Interfaces;

namespace VitalSense.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardMetricsResponse> GetMetricsAsync(Guid dieticianId, DateTime utcNow)
    {
        // Normalize to first day boundaries in UTC
        var firstOfThisMonth = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstOfNextMonth = firstOfThisMonth.AddMonths(1);
        var firstOfLastMonth = firstOfThisMonth.AddMonths(-1);

        // Execute sequentially to avoid concurrent operations on the same DbContext instance
        var totalClients = await _context.Clients.CountAsync(c => c.DieticianId == dieticianId);

        var activeClients = await _context.MealPlans
            .Where(mp => mp.DieticianId == dieticianId && mp.StartDate <= utcNow && mp.EndDate >= utcNow)
            .Select(mp => mp.ClientId)
            .Distinct()
            .CountAsync();

        var newThis = await _context.Clients
            .CountAsync(c => c.DieticianId == dieticianId && c.CreatedAt >= firstOfThisMonth && c.CreatedAt < firstOfNextMonth);

        var newLast = await _context.Clients
            .CountAsync(c => c.DieticianId == dieticianId && c.CreatedAt >= firstOfLastMonth && c.CreatedAt < firstOfThisMonth);

        double changePercent;
        if (newLast == 0)
        {
            // Define: from 0 to X -> X*100% growth (unbounded), from 0 to 0 -> 0%
            changePercent = newThis == 0 ? 0 : newThis * 100.0;
        }
        else
        {
            changePercent = ((double)newThis - newLast) / newLast * 100.0;
        }
        var rounded = Math.Round(changePercent, 2);
        var formatted = (rounded > 0 ? $"+{rounded}" : rounded.ToString()) + "%";

        return new DashboardMetricsResponse
        {
            TotalClients = totalClients,
            ActiveClients = activeClients,
            NewClientsThisMonth = newThis,
            NewClientsLastMonth = newLast,
            NewClientsChangePercent = rounded,
            NewClientsChangePercentFormatted = formatted
        };
    }
}
