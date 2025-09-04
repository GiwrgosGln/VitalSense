using VitalSense.Domain.Entities;

namespace VitalSense.Application.Interfaces;

public interface IQuestionnaireTemplateService
{
    Task<QuestionnaireTemplate?> GetByIdAsync(Guid templateId);
    Task<QuestionnaireTemplate> CreateAsync(QuestionnaireTemplate template);
    Task<QuestionnaireTemplate?> UpdateAsync(Guid templateId, QuestionnaireTemplate template);
    Task<bool> DeleteAsync(Guid templateId);
    Task<IEnumerable<QuestionnaireTemplate>> GetAllByDieticianAsync(Guid dieticianId);
}
