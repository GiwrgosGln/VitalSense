using System.IO;
using VitalSense.Application.DTOs;
using VitalSense.Domain.Entities;

namespace VitalSense.Application.Services;

public interface IClientService
{
    Task<ClientResponse?> GetByIdAsync(Guid clientId);
    Task<ClientResponse> CreateAsync(CreateClientRequest dto);
    Task<ClientResponse?> UpdateAsync(Guid clientId, UpdateClientRequest dto);
    Task<bool> DeleteAsync(Guid clientId);
    Task<IEnumerable<ClientResponse>> GetAllByDieticianAsync(Guid dieticianId, int pageNumber = 1, int pageSize = 20);
    Task<IEnumerable<ClientResponse>> SearchAsync(Guid dieticianId, string query, int limit = 20);
    Task<ImportClientsResponse> ImportFromExcelAsync(Guid dieticianId, Stream excelStream);
}