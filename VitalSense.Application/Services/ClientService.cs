using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace VitalSense.Application.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Client?> GetByIdAsync(Guid clientId)
        => await _context.Clients.FindAsync(clientId);

    public async Task<Client> CreateAsync(Client client)
    {
        client.Id = Guid.NewGuid();
        await _context.Clients.AddAsync(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<Client?> UpdateAsync(Guid clientId, Client updatedClient)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return null;

        client.FirstName = updatedClient.FirstName;
        client.LastName = updatedClient.LastName;
        client.Email = updatedClient.Email;
        client.Phone = updatedClient.Phone;
        client.DateOfBirth = updatedClient.DateOfBirth;
        client.Gender = updatedClient.Gender;
        client.HasCard = updatedClient.HasCard;
        client.Notes = updatedClient.Notes;
        client.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return client;
    }

    public async Task<bool> DeleteAsync(Guid clientId)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null) return false;

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Client>> GetAllByDieticianAsync(Guid dieticianId)
    {
        return await _context.Clients.Where(c => c.DieticianId == dieticianId).ToListAsync();
    }

    public async Task<IEnumerable<Client>> SearchAsync(Guid dieticianId, string query, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<Client>();
        }

        query = query.Trim();
        var lowered = query.ToLowerInvariant();
        return await _context.Clients
            .Where(c => c.DieticianId == dieticianId && (
                c.FirstName.ToLower().Contains(lowered) ||
                c.LastName.ToLower().Contains(lowered) ||
                c.Email.ToLower().Contains(lowered) ||
                c.Phone.ToLower().Contains(lowered)
            ))
            .OrderBy(c => c.FirstName)
            .ThenBy(c => c.LastName)
            .Take(limit)
            .ToListAsync();
    }
}