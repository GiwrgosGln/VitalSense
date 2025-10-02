using Microsoft.EntityFrameworkCore;
using VitalSense.Application.Interfaces;
using VitalSense.Domain.Entities;
using VitalSense.Infrastructure.Data;
using VitalSense.Shared.Services;

namespace VitalSense.Application.Services;

public class QuestionnaireTemplateService : IQuestionnaireTemplateService
{
    private readonly AppDbContext _context;
    private readonly IEncryptionService _encryptionService;

    public QuestionnaireTemplateService(AppDbContext context, IEncryptionService encryptionService)
    {
        _context = context;
        _encryptionService = encryptionService;
    }
    
    // Questionnaire Submission methods
    public async Task<QuestionnaireSubmission> SubmitQuestionnaireAsync(QuestionnaireSubmission submission)
    {
        submission.Id = Guid.NewGuid();
        submission.SubmittedAt = DateTime.UtcNow;
        
        foreach (var answer in submission.Answers)
        {
            answer.Id = Guid.NewGuid();
            answer.SubmissionId = submission.Id;
            // Encrypt the answer text before saving
            answer.AnswerText = _encryptionService.Protect(answer.AnswerText);
        }
        
        await _context.Set<QuestionnaireSubmission>().AddAsync(submission);
        await _context.SaveChangesAsync();
        
        return submission;
    }
    
    public async Task<IEnumerable<QuestionnaireSubmission>> GetSubmissionsByClientIdAsync(Guid clientId)
    {
        var submissions = await _context.Set<QuestionnaireSubmission>()
            .Include(s => s.Answers)
            .Include(s => s.Template)
                .ThenInclude(t => t.Questions)
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();
            
        // Decrypt all answer texts
        foreach (var submission in submissions)
        {
            foreach (var answer in submission.Answers)
            {
                answer.AnswerText = _encryptionService.Unprotect(answer.AnswerText);
            }
        }
        
        return submissions;
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
            var existingTemplate = await _context.QuestionnaireTemplates
                .Include(qt => qt.Questions)
                .FirstOrDefaultAsync(qt => qt.Id == templateId);
                
            if (existingTemplate == null) return null;
            
            // Update template properties
            existingTemplate.Title = updatedTemplate.Title;
            existingTemplate.Description = updatedTemplate.Description;
            existingTemplate.UpdatedAt = DateTime.UtcNow;
            
            // Remove all existing questions
            _context.Set<QuestionnaireQuestion>().RemoveRange(existingTemplate.Questions);
            
            // Add updated questions
            foreach (var updatedQuestion in updatedTemplate.Questions)
            {
                var questionId = updatedQuestion.Id != Guid.Empty ? updatedQuestion.Id : Guid.NewGuid();
                
                var question = new QuestionnaireQuestion
                {
                    Id = questionId,
                    TemplateId = templateId,
                    QuestionText = updatedQuestion.QuestionText,
                    Order = updatedQuestion.Order,
                    IsRequired = updatedQuestion.IsRequired
                };
                
                await _context.Set<QuestionnaireQuestion>().AddAsync(question);
            }
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            // Return the updated template with questions
            var result = await _context.QuestionnaireTemplates
                .Include(qt => qt.Questions)
                .AsNoTracking()
                .FirstOrDefaultAsync(qt => qt.Id == templateId);
                
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
