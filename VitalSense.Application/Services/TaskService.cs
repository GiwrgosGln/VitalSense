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
            .Where(t => t.DieticianId == dieticianId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToResponse(t))
            .ToListAsync();
    }

    public async Task<TaskResponse?> GetByIdAsync(Guid dieticianId, Guid taskId)
    {
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId && t.DieticianId == dieticianId);
        return task == null ? null : ToResponse(task);
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
