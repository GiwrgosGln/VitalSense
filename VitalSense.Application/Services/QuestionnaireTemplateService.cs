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
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var templateExists = await _context.QuestionnaireTemplates
                .AsNoTracking()
                .AnyAsync(qt => qt.Id == templateId);
                
            if (!templateExists) return null;
            
            var questionsSql = $"DELETE FROM questionnaire_questions WHERE template_id = '{templateId}'";
            await _context.Database.ExecuteSqlRawAsync(questionsSql);
            
            var templateSql = $@"
                UPDATE questionnaire_templates 
                SET title = '{updatedTemplate.Title.Replace("'", "''")}', 
                    description = '{(updatedTemplate.Description?.Replace("'", "''") ?? "")}', 
                    updated_at = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}'
                WHERE id = '{templateId}'";
            
            await _context.Database.ExecuteSqlRawAsync(templateSql);
            
            foreach (var updatedQuestion in updatedTemplate.Questions)
            {
                var questionId = updatedQuestion.Id != Guid.Empty ? updatedQuestion.Id : Guid.NewGuid();
                var isRequired = updatedQuestion.IsRequired ? 1 : 0;
                
                var insertQuestionSql = $@"
                    INSERT INTO questionnaire_questions (id, template_id, question_text, [order], is_required)
                    VALUES ('{questionId}', '{templateId}', '{updatedQuestion.QuestionText.Replace("'", "''")}', {updatedQuestion.Order}, {isRequired})";
                
                await _context.Database.ExecuteSqlRawAsync(insertQuestionSql);
            }
            
            await transaction.CommitAsync();
            
            var result = await _context.QuestionnaireTemplates
                .AsNoTracking()
                .FirstOrDefaultAsync(qt => qt.Id == templateId);
                
            if (result != null)
            {
                var questions = await _context.Set<QuestionnaireQuestion>()
                    .AsNoTracking()
                    .Where(q => q.TemplateId == templateId)
                    .ToListAsync();
                    
                result.Questions = questions;
            }
                
            return result;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
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
