using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VitalSense.Api.Endpoints;
using VitalSense.Application.DTOs.Questionnaire;
using VitalSense.Application.Interfaces;
using VitalSense.Domain.Entities;

namespace VitalSense.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class QuestionnaireTemplateController : ControllerBase
{
    private readonly IQuestionnaireTemplateService _questionnaireTemplateService;

    public QuestionnaireTemplateController(IQuestionnaireTemplateService questionnaireTemplateService)
    {
        _questionnaireTemplateService = questionnaireTemplateService;
    }

    [HttpGet(ApiEndpoints.QuestionnaireTemplates.GetAll)]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireTemplateResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var templates = await _questionnaireTemplateService.GetAllByDieticianAsync(dieticianId);
        var response = templates.Select(MapToResponse);
        
        return Ok(response);
    }

    [HttpGet(ApiEndpoints.QuestionnaireTemplates.GetById)]
    [ProducesResponseType(typeof(QuestionnaireTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById([FromRoute] Guid templateId)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var template = await _questionnaireTemplateService.GetByIdAsync(templateId);
        if (template == null) return NotFound();

        if (template.DieticianId != dieticianId)
            return Forbid();

        return Ok(MapToResponse(template));
    }

    [HttpPost(ApiEndpoints.QuestionnaireTemplates.Create)]
    [ProducesResponseType(typeof(QuestionnaireTemplateResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateQuestionnaireTemplateRequest request)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var template = new QuestionnaireTemplate
        {
            Title = request.Title,
            Description = request.Description,
            DieticianId = dieticianId,
            Questions = request.Questions.Select(q => new QuestionnaireQuestion
            {
                QuestionText = q.QuestionText,
                Order = q.Order,
                IsRequired = q.IsRequired
            }).ToList()
        };

        var created = await _questionnaireTemplateService.CreateAsync(template);
        var response = MapToResponse(created);

        return CreatedAtAction(nameof(GetById), new { templateId = response.Id }, response);
    }

    [HttpPut(ApiEndpoints.QuestionnaireTemplates.Update)]
    [ProducesResponseType(typeof(QuestionnaireTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update([FromRoute] Guid templateId, [FromBody] UpdateQuestionnaireTemplateRequest request)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var existingTemplate = await _questionnaireTemplateService.GetByIdAsync(templateId);
        if (existingTemplate == null) return NotFound();
        if (existingTemplate.DieticianId != dieticianId) return Forbid();

        var template = new QuestionnaireTemplate
        {
            Id = templateId,
            Title = request.Title,
            Description = request.Description,
            DieticianId = dieticianId,
            Questions = request.Questions.Select(q => new QuestionnaireQuestion
            {
                Id = q.Id ?? Guid.Empty,
                QuestionText = q.QuestionText,
                Order = q.Order,
                IsRequired = q.IsRequired
            }).ToList()
        };

        var updated = await _questionnaireTemplateService.UpdateAsync(templateId, template);
        if (updated == null) return NotFound();

        return Ok(MapToResponse(updated));
    }

    [HttpDelete(ApiEndpoints.QuestionnaireTemplates.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete([FromRoute] Guid templateId)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }

        var existingTemplate = await _questionnaireTemplateService.GetByIdAsync(templateId);
        if (existingTemplate == null) return NotFound();
        if (existingTemplate.DieticianId != dieticianId) return Forbid();

        var deleted = await _questionnaireTemplateService.DeleteAsync(templateId);
        if (!deleted) return NotFound();
        
        return NoContent();
    }

    private QuestionnaireTemplateResponse MapToResponse(QuestionnaireTemplate template)
    {
        return new QuestionnaireTemplateResponse
        {
            Id = template.Id,
            Title = template.Title,
            Description = template.Description,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            Questions = template.Questions
                .OrderBy(q => q.Order)
                .Select(q => new QuestionnaireQuestionResponse
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Order = q.Order,
                    IsRequired = q.IsRequired
                }).ToList()
        };
    }
}
