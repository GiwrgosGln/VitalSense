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
    
    // Questionnaire Submission endpoints
    [HttpPost(ApiEndpoints.QuestionnaireTemplates.SubmitQuestionnaire)]
    [ProducesResponseType(typeof(QuestionnaireSubmissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitQuestionnaire([FromBody] QuestionnaireSubmissionRequest request)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }
        
        // Verify template exists and belongs to the dietician
        var template = await _questionnaireTemplateService.GetByIdAsync(request.TemplateId);
        if (template == null) return NotFound("Questionnaire template not found");
        if (template.DieticianId != dieticianId) return Forbid();
        
        // Create submission
        var submission = new QuestionnaireSubmission
        {
            TemplateId = request.TemplateId,
            DieticianId = dieticianId,
            ClientId = request.ClientId,
            Answers = request.Answers.Select(a => new QuestionnaireAnswer
            {
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText
            }).ToList()
        };
        
        // Validate that all required questions are answered
        var requiredQuestions = template.Questions.Where(q => q.IsRequired).Select(q => q.Id).ToHashSet();
        var answeredQuestions = submission.Answers.Select(a => a.QuestionId).ToHashSet();
        var missingRequiredQuestions = requiredQuestions.Except(answeredQuestions).ToList();
        
        if (missingRequiredQuestions.Any())
        {
            return BadRequest("Some required questions are not answered");
        }
        
        var created = await _questionnaireTemplateService.SubmitQuestionnaireAsync(submission);
        var response = MapSubmissionToResponse(created, template);
        
        return CreatedAtAction(nameof(GetSubmissionsByClientId), new { clientId = response.ClientId }, response);
    }
    
    [HttpGet(ApiEndpoints.QuestionnaireTemplates.GetSubmissionsByClientId)]
    [ProducesResponseType(typeof(IEnumerable<QuestionnaireSubmissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubmissionsByClientId([FromRoute] Guid clientId)
    {
        var dieticianIdClaim = User.FindFirst("userid")?.Value;
        if (string.IsNullOrEmpty(dieticianIdClaim) || !Guid.TryParse(dieticianIdClaim, out var dieticianId))
        {
            return Unauthorized();
        }
        
        var submissions = await _questionnaireTemplateService.GetSubmissionsByClientIdAsync(clientId);
        
        // Filter out submissions that don't belong to the dietician
        var filteredSubmissions = submissions.Where(s => s.DieticianId == dieticianId).ToList();
        
        var response = filteredSubmissions.Select(s => MapSubmissionToResponse(s, s.Template)).ToList();
        
        return Ok(response);
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
    
    private QuestionnaireSubmissionResponse MapSubmissionToResponse(QuestionnaireSubmission submission, QuestionnaireTemplate template)
    {
        // Create a dictionary to lookup questions by ID
        var questionsById = template.Questions.ToDictionary(q => q.Id, q => q);
        
        return new QuestionnaireSubmissionResponse
        {
            Id = submission.Id,
            TemplateId = submission.TemplateId,
            TemplateTitle = template.Title,
            ClientId = submission.ClientId,
            SubmittedAt = submission.SubmittedAt,
            Answers = submission.Answers.Select(a => new QuestionnaireAnswerResponse
            {
                QuestionId = a.QuestionId,
                QuestionText = questionsById.TryGetValue(a.QuestionId, out var question) ? question.QuestionText : "Unknown Question",
                AnswerText = a.AnswerText
            }).ToList()
        };
    }
}
