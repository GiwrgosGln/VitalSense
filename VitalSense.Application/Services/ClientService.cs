using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;

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
    
    public async Task<ImportClientsResponse> ImportFromExcelAsync(Guid dieticianId, Stream excelStream)
    {
        var response = new ImportClientsResponse();
        var clients = new List<Client>();
        
        try
        {
            IWorkbook workbook;
            try
            {
                // (Excel 2007+)
                workbook = new XSSFWorkbook(excelStream);
            }
            catch
            {
                excelStream.Position = 0;
                // (Excel 97-2003)
                workbook = new HSSFWorkbook(excelStream);
            }

            ISheet sheet = workbook.GetSheetAt(0);

            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row == null) continue;
                
                try
                {
                    // Required fields
                    string firstName = GetStringCellValue(row.GetCell(0));
                    string lastName = GetStringCellValue(row.GetCell(1));
                    
                    // Optional fields
                    string phone = GetStringCellValue(row.GetCell(2), allowNull: true);
                    string email = GetStringCellValue(row.GetCell(3), allowNull: true);
                    string notes = GetStringCellValue(row.GetCell(4), allowNull: true);
                    
                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                    {
                        response.Errors.Add($"Row {rowIndex + 1}: First name and last name are required.");
                        response.FailureCount++;
                        continue;
                    }
                    
                    var client = new Client
                    {
                        Id = Guid.NewGuid(),
                        FirstName = firstName,
                        LastName = lastName,
                        Phone = phone ?? string.Empty,
                        Email = email ?? string.Empty,
                        Notes = notes,
                        DieticianId = dieticianId,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    clients.Add(client);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.Errors.Add($"Row {rowIndex + 1}: {ex.Message}");
                    response.FailureCount++;
                }
            }
            
            response.TotalProcessed = response.SuccessCount + response.FailureCount;
            
            if (clients.Any())
            {
                await _context.Clients.AddRangeAsync(clients);
                await _context.SaveChangesAsync();
                
                foreach (var client in clients)
                {
                    response.ImportedClients.Add(new ClientResponse
                    {
                        Id = client.Id,
                        FirstName = client.FirstName,
                        LastName = client.LastName,
                        Email = client.Email,
                        Phone = client.Phone,
                        DateOfBirth = client.DateOfBirth,
                        Gender = client.Gender,
                        HasCard = client.HasCard,
                        Notes = client.Notes,
                        CreatedAt = client.CreatedAt,
                        UpdatedAt = client.UpdatedAt
                    });
                }
            }
        }
        catch (Exception ex)
        {
            response.Errors.Add($"Error processing Excel file: {ex.Message}");
            response.FailureCount++;
        }
        
        return response;
    }
    
    private string GetStringCellValue(ICell cell, bool allowNull = false)
    {
        if (cell == null)
        {
            if (allowNull) return string.Empty;
            return string.Empty;
        }
        
        switch (cell.CellType)
        {
            case CellType.String:
                return cell.StringCellValue;
            case CellType.Numeric:
                return cell.NumericCellValue.ToString();
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            case CellType.Formula:
                switch (cell.CachedFormulaResultType)
                {
                    case CellType.String:
                        return cell.StringCellValue;
                    case CellType.Numeric:
                        return cell.NumericCellValue.ToString();
                    default:
                        return cell.ToString() ?? string.Empty;
                }
            default:
                return cell.ToString() ?? string.Empty;
        }
    }
}