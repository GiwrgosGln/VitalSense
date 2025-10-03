using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace VitalSense.Application.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskResponse> CreateAsync(Guid dieticianId, CreateTaskRequest request)
    {
        var now = DateTime.UtcNow;
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            IsCompleted = false,
            CreatedAt = now,
            UpdatedAt = now,
            DieticianId = dieticianId,
            ClientId = request.ClientId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return ToResponse(task);
    }

    public async Task<IEnumerable<TaskResponse>> GetAllAsync(Guid dieticianId)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.DieticianId == dieticianId)
            .OrderByDescending(t => t.CreatedAt)
            .Join(
                _context.Clients,
                task => task.ClientId,
                client => client.Id,
                (task, client) => new { task, client }
            )
            .Select(tc => new TaskResponse
            {
                Id = tc.task.Id,
                Title = tc.task.Title,
                Description = tc.task.Description,
                IsCompleted = tc.task.IsCompleted,
                DueDate = tc.task.DueDate,
                CreatedAt = tc.task.CreatedAt,
                UpdatedAt = tc.task.UpdatedAt,
                ClientId = tc.task.ClientId,
                ClientName = tc.client.FirstName,
                ClientSurname = tc.client.LastName
            })
            .ToListAsync();
    }

    public async Task<TaskResponse?> GetByIdAsync(Guid dieticianId, Guid taskId)
    {
        var result = await _context.Tasks
            .AsNoTracking()
            .Where(t => t.Id == taskId && t.DieticianId == dieticianId)
            .Join(
                _context.Clients,
                task => task.ClientId,
                client => client.Id,
                (task, client) => new { task, client }
            )
            .Select(tc => new TaskResponse
            {
                Id = tc.task.Id,
                Title = tc.task.Title,
                Description = tc.task.Description,
                IsCompleted = tc.task.IsCompleted,
                DueDate = tc.task.DueDate,
                CreatedAt = tc.task.CreatedAt,
                UpdatedAt = tc.task.UpdatedAt,
                ClientId = tc.task.ClientId,
                ClientName = tc.client.FirstName,
                ClientSurname = tc.client.LastName
            })
            .FirstOrDefaultAsync();

        return result;
    }

    public async Task<TaskResponse?> ToggleCompleteAsync(Guid dieticianId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.DieticianId == dieticianId);
        if (task == null) return null;
        task.IsCompleted = !task.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(Guid dieticianId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.DieticianId == dieticianId);
        if (task == null) return false;
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    private static TaskResponse ToResponse(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted,
        DueDate = t.DueDate,
        CreatedAt = t.CreatedAt,
    UpdatedAt = t.UpdatedAt,
    ClientId = t.ClientId
    };
}
