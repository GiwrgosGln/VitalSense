using Microsoft.EntityFrameworkCore;
using VitalSense.Application.Interfaces;
using VitalSense.Domain.Entities;
using VitalSense.Infrastructure.Data;

namespace VitalSense.Application.Services;

public class QuestionnaireTemplateService : IQuestionnaireTemplateService
{
    private readonly AppDbContext _context;

    public QuestionnaireTemplateService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QuestionnaireTemplate?> GetByIdAsync(Guid templateId)
    {
        return await _context.QuestionnaireTemplates
            .Include(qt => qt.Questions)
            .FirstOrDefaultAsync(qt => qt.Id == templateId);
    }

    public async Task<QuestionnaireTemplate> CreateAsync(QuestionnaireTemplate template)
    {
        template.Id = Guid.NewGuid();
        template.CreatedAt = DateTime.UtcNow;
        
        foreach (var question in template.Questions)
        {
            question.Id = Guid.NewGuid();
            question.TemplateId = template.Id;
        }
        
        await _context.QuestionnaireTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
        
        return template;
    }

    public async Task<QuestionnaireTemplate?> UpdateAsync(Guid templateId, QuestionnaireTemplate updatedTemplate)
    {
        var template = await _context.QuestionnaireTemplates
            .Include(qt => qt.Questions)
            .FirstOrDefaultAsync(qt => qt.Id == templateId);
            
        if (template == null) return null;

        template.Title = updatedTemplate.Title;
        template.Description = updatedTemplate.Description;
        template.UpdatedAt = DateTime.UtcNow;

        var existingQuestionIds = template.Questions.Select(q => q.Id).ToList();
        var updatedQuestionIds = updatedTemplate.Questions.Where(q => q.Id != Guid.Empty).Select(q => q.Id).ToList();
        
        var questionsToRemove = template.Questions.Where(q => !updatedQuestionIds.Contains(q.Id)).ToList();
        foreach (var question in questionsToRemove)
        {
            _context.Remove(question);
        }
        
        foreach (var updatedQuestion in updatedTemplate.Questions)
        {
            var existingQuestion = template.Questions.FirstOrDefault(q => q.Id == updatedQuestion.Id);
            
            if (existingQuestion != null)
            {
                existingQuestion.QuestionText = updatedQuestion.QuestionText;
                existingQuestion.Order = updatedQuestion.Order;
                existingQuestion.IsRequired = updatedQuestion.IsRequired;
            }
            else
            {
                var newQuestion = new QuestionnaireQuestion
                {
                    Id = Guid.NewGuid(),
                    TemplateId = template.Id,
                    QuestionText = updatedQuestion.QuestionText,
                    Order = updatedQuestion.Order,
                    IsRequired = updatedQuestion.IsRequired
                };
                
                template.Questions.Add(newQuestion);
            }
        }
        
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteAsync(Guid templateId)
    {
        var template = await _context.QuestionnaireTemplates
            .Include(qt => qt.Questions)
            .FirstOrDefaultAsync(qt => qt.Id == templateId);
            
        if (template == null) return false;

        _context.QuestionnaireTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<QuestionnaireTemplate>> GetAllByDieticianAsync(Guid dieticianId)
    {
        return await _context.QuestionnaireTemplates
            .Include(qt => qt.Questions)
            .Where(qt => qt.DieticianId == dieticianId)
            .OrderByDescending(qt => qt.CreatedAt)
            .ToListAsync();
    }
}
