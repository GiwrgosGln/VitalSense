using VitalSense.Infrastructure.Data;
using VitalSense.Domain.Entities;
using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace VitalSense.Application.Services;

public class MealPlanService : IMealPlanService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly string _modelName;
    private readonly ILogger<MealPlanService> _logger;

    private static class OpenRouterConstants
    {
        public const string JsonStartTag = "```json";
        public const string CodeBlockTag = "```";
        public const string ApiResponseChoicesPath = "choices";
        public const string ApiResponseMessagePath = "message";
        public const string ApiResponseContentPath = "content";
    }

    public MealPlanService(
        AppDbContext context, 
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<MealPlanService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"] ?? throw new ArgumentNullException("OpenRouter:ApiKey not configured");
        _apiUrl = configuration["OpenRouter:ApiUrl"] ?? "https://openrouter.ai/api/v1/chat/completions";
        _modelName = configuration["OpenRouter:Model"] ?? "google/gemini-2.5-flash-lite-preview-09-2025";
        
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<MealPlanResponse> CreateAsync(CreateMealPlanRequest request)
    {
        var now = DateTime.UtcNow;
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DieticianId = request.DieticianId,
            ClientId = request.ClientId,
            Days = request.Days.Select(day => new MealDay
            {
                Id = Guid.NewGuid(),
                Title = day.Title,
                Meals = day.Meals.Select(meal => new Meal
                {
                    Id = Guid.NewGuid(),
                    Title = meal.Title,
                    Time = meal.Time ?? "",
                    Description = meal.Description,
                    Protein = meal.Protein,
                    Carbs = meal.Carbs,
                    Fats = meal.Fats,
                    Calories = meal.Calories
                }).ToList()
            }).ToList(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.MealPlans.Add(mealPlan);
        await _context.SaveChangesAsync();

        return new MealPlanResponse
        {
            Id = mealPlan.Id,
            Title = mealPlan.Title,
            StartDate = mealPlan.StartDate,
            EndDate = mealPlan.EndDate,
            DieticianId = mealPlan.DieticianId,
            ClientId = mealPlan.ClientId,
            Days = mealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        };
    }

    public async Task<MealPlanResponse?> GetByIdAsync(Guid mealPlanId)
    {
        var mealPlan = await _context.MealPlans
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

        if (mealPlan == null) return null;

        return new MealPlanResponse
        {
            Id = mealPlan.Id,
            Title = mealPlan.Title,
            StartDate = mealPlan.StartDate,
            EndDate = mealPlan.EndDate,
            DieticianId = mealPlan.DieticianId,
            ClientId = mealPlan.ClientId,
            Days = mealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        };
    }

    public async Task<List<MealPlanResponse>> GetAllAsync(Guid clientId)
    {
        var mealPlans = await _context.MealPlans
            .Where(mp => mp.ClientId == clientId)
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
            .ToListAsync();

        return mealPlans.Select(mealPlan => new MealPlanResponse
        {
            Id = mealPlan.Id,
            Title = mealPlan.Title,
            StartDate = mealPlan.StartDate,
            EndDate = mealPlan.EndDate,
            DieticianId = mealPlan.DieticianId,
            ClientId = mealPlan.ClientId,
            Days = mealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        }).ToList();
    }

    public async Task<MealPlanResponse?> GetActiveMealPlanAsync(Guid clientId)
    {
        var now = DateTime.UtcNow.Date;
        var latestMealPlan = await _context.MealPlans
            .Where(mp => mp.ClientId == clientId && mp.StartDate.Date <= now && mp.EndDate.Date >= now)
            .OrderByDescending(mp => mp.UpdatedAt)
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
            .FirstOrDefaultAsync();

        if (latestMealPlan == null) return null;

        return new MealPlanResponse
        {
            Id = latestMealPlan.Id,
            Title = latestMealPlan.Title,
            StartDate = latestMealPlan.StartDate,
            EndDate = latestMealPlan.EndDate,
            DieticianId = latestMealPlan.DieticianId,
            ClientId = latestMealPlan.ClientId,
            Days = latestMealPlan.Days.Select(d => new MealDayResponse
            {
                Id = d.Id,
                Title = d.Title,
                Meals = d.Meals.Select(m => new MealResponse
                {
                    Id = m.Id,
                    Title = m.Title,
                    Time = m.Time,
                    Description = m.Description,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fats = m.Fats,
                    Calories = m.Calories
                }).ToList()
            }).ToList()
        };
    }

    public async Task<MealPlanResponse?> UpdateAsync(Guid mealPlanId, UpdateMealPlanRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var existingMealPlan = await _context.MealPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

            if (existingMealPlan == null)
                return null;

            var mealsSql = $"DELETE FROM meals WHERE meal_day_id IN (SELECT id FROM meal_days WHERE meal_plan_id = '{mealPlanId}')";
            await _context.Database.ExecuteSqlRawAsync(mealsSql);

            var daysSql = $"DELETE FROM meal_days WHERE meal_plan_id = '{mealPlanId}'";
            await _context.Database.ExecuteSqlRawAsync(daysSql);

            var updatedMealPlan = await _context.MealPlans.FindAsync(mealPlanId);
            if (updatedMealPlan == null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            updatedMealPlan.Title = request.Title;
            updatedMealPlan.StartDate = request.StartDate;
            updatedMealPlan.EndDate = request.EndDate;
            updatedMealPlan.DieticianId = request.DieticianId;
            updatedMealPlan.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            foreach (var dayRequest in request.Days)
            {
                var day = new MealDay
                {
                    Id = dayRequest.Id ?? Guid.NewGuid(),
                    MealPlanId = mealPlanId,
                    Title = dayRequest.Title,
                };

                _context.MealDays.Add(day);
                await _context.SaveChangesAsync();

                foreach (var mealRequest in dayRequest.Meals)
                {
                    var meal = new Meal
                    {
                        Id = mealRequest.Id ?? Guid.NewGuid(),
                        MealDayId = day.Id,
                        Title = mealRequest.Title,
                        Time = mealRequest.Time ?? "",
                        Description = mealRequest.Description,
                        Protein = mealRequest.Protein,
                        Carbs = mealRequest.Carbs,
                        Fats = mealRequest.Fats,
                        Calories = mealRequest.Calories
                    };

                    _context.Meals.Add(meal);
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            var result = await _context.MealPlans
                .Include(mp => mp.Days)
                    .ThenInclude(d => d.Meals)
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

            if (result == null)
                return null;

            return new MealPlanResponse
            {
                Id = result.Id,
                Title = result.Title,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                DieticianId = result.DieticianId,
                ClientId = result.ClientId,
                Days = result.Days.Select(d => new MealDayResponse
                {
                    Id = d.Id,
                    Title = d.Title,
                    Meals = d.Meals.Select(m => new MealResponse
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Time = m.Time,
                        Description = m.Description,
                        Protein = m.Protein,
                        Carbs = m.Carbs,
                        Fats = m.Fats,
                        Calories = m.Calories
                    }).ToList()
                }).ToList()
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
    public async Task DeleteAsync(Guid mealPlanId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var mealsSql = $"DELETE FROM meals WHERE meal_day_id IN (SELECT id FROM meal_days WHERE meal_plan_id = '{mealPlanId}')";
            await _context.Database.ExecuteSqlRawAsync(mealsSql);
            
            var daysSql = $"DELETE FROM meal_days WHERE meal_plan_id = '{mealPlanId}'";
            await _context.Database.ExecuteSqlRawAsync(daysSql);
            
            var planSql = $"DELETE FROM meal_plans WHERE id = '{mealPlanId}'";
            await _context.Database.ExecuteSqlRawAsync(planSql);
            
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<CreateMealPlanRequest> ConvertExcelToMealPlanAsync(byte[] excelFileData, string fileName, Guid dieticianId, Guid clientId)
    {
        try
        {
            var excelContentAsText = ConvertExcelToString(excelFileData);

            var prompt = BuildPrompt(excelContentAsText, dieticianId, clientId);
            var requestJson = CreateRequestJson(prompt);
            var response = await SendRequest(requestJson);

            var mealPlanJson = ExtractJsonFromResponse(response);
            
            var mealPlanRequest = DeserializeAndValidateMealPlan(
                mealPlanJson, fileName, dieticianId, clientId);
            
            return mealPlanRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel file with OpenRouter API");
            throw new ApplicationException($"Error processing Excel file with OpenRouter API: {ex.Message}", ex);
        }
    }

    private string BuildPrompt(string excelContentAsText, Guid dieticianId, Guid clientId)
    {
        return $@"I'm sending you text data extracted from an Excel file containing a meal plan. Please convert it to a JSON object with the following structure:

    ```csharp
    public class CreateMealPlanRequest
    {{
        public string Title {{ get; set; }} = string.Empty;
        public DateTime StartDate {{ get; set; }}
        public DateTime EndDate {{ get; set; }}
        public Guid DieticianId {{ get; set; }}
        public Guid ClientId {{ get; set; }}
        public List<MealDayRequest> Days {{ get; set; }} = new();
    }}

    public class MealDayRequest
    {{
        public string Title {{ get; set; }} = string.Empty;
        public List<MealRequest> Meals {{ get; set; }} = new();
    }}

    public class MealRequest
    {{
        public string Title {{ get; set; }} = string.Empty;
        public string? Time {{ get; set; }}
        public string? Description {{ get; set; }}
        public float Protein {{ get; set; }}
        public float Carbs {{ get; set; }}
        public float Fats {{ get; set; }}
        public float Calories {{ get; set; }}
    }}
    ```

    Here is the meal plan data:

    {excelContentAsText}

    Please use the following values for DieticianId and ClientId:
    - DieticianId: {dieticianId}
    - ClientId: {clientId}

    Use the filename as the meal plan Title if not otherwise specified in the data.
    Infer the StartDate and EndDate from the data if available, otherwise use the current date for StartDate and 7 days later for EndDate.

    Respond ONLY with the valid JSON object, properly escaped and nothing else.";
    }

    private string CreateRequestJson(string prompt)
    {
        var openRouterRequest = new
        {
            model = _modelName,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt }
                    }
                }
            },
            temperature = 0.2,
            top_p = 1.0,
            max_tokens = 8192
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        return JsonSerializer.Serialize(openRouterRequest, jsonOptions);
    }

    private async Task<string> SendRequest(string requestJson)
    {
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        var apiUrl = _apiUrl;
        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        request.Content = requestContent;
        
        var response = await SendWithRetryAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("OpenRouter API error: {StatusCode} - {ErrorContent}", 
                response.StatusCode, errorContent);
            throw new InvalidOperationException($"OpenRouter API returned status code {response.StatusCode}: {errorContent}");
        }
        
        return await response.Content.ReadAsStringAsync();
    }

    private string ExtractJsonFromResponse(string responseContent)
    {
        try
        {
            _logger.LogDebug("Processing response from OpenRouter API");
            
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            if (!responseJson.TryGetProperty(OpenRouterConstants.ApiResponseChoicesPath, out var choices) || 
                choices.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Missing or empty choices array in API response");
            }
            
            if (!choices[0].TryGetProperty(OpenRouterConstants.ApiResponseMessagePath, out var message))
            {
                throw new InvalidOperationException("Missing message object in API response");
            }
            
            if (!message.TryGetProperty(OpenRouterConstants.ApiResponseContentPath, out var content) ||
                content.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException("Missing or invalid content in API response");
            }
            
            var jsonContent = content.GetString() ?? "";
            return CleanJsonResponse(jsonContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse OpenRouter API response as JSON", ex);
        }
    }

    private string CleanJsonResponse(string jsonText)
    {
        var cleanedJson = jsonText.Trim();
        
        if (cleanedJson.StartsWith(OpenRouterConstants.JsonStartTag))
        {
            cleanedJson = cleanedJson[OpenRouterConstants.JsonStartTag.Length..];
        }
        else if (cleanedJson.StartsWith(OpenRouterConstants.CodeBlockTag))
        {
            cleanedJson = cleanedJson[OpenRouterConstants.CodeBlockTag.Length..];
        }
        
        if (cleanedJson.EndsWith(OpenRouterConstants.CodeBlockTag))
        {
            cleanedJson = cleanedJson[..^OpenRouterConstants.CodeBlockTag.Length];
        }
        
        return cleanedJson.Trim();
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(HttpRequestMessage request, int maxRetries = 3, int delayMilliseconds = 2000)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                attempts++;
                var requestClone = await CloneHttpRequestMessageAsync(request);
                var response = await _httpClient.SendAsync(requestClone);

                if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests || attempts >= maxRetries)
                {
                    return response;
                }

                _logger.LogWarning("Rate limited by OpenRouter API. Retrying in {DelayMs}ms... (Attempt {Attempt}/{MaxRetries})", 
                    delayMilliseconds, attempts, maxRetries);
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2;
            }
            catch (Exception ex)
            {
                if (attempts >= maxRetries)
                {
                    throw new ApplicationException($"Failed to send request to OpenRouter API after {attempts} attempts.", ex);
                }
                _logger.LogWarning(ex, "Request to OpenRouter API failed. Retrying in {DelayMs}ms... (Attempt {Attempt}/{MaxRetries})",
                    delayMilliseconds, attempts, maxRetries);
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2;
            }
        }
    }

    private async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri);

        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            if (req.Content.Headers != null)
                foreach (var h in req.Content.Headers)
                    clone.Content.Headers.Add(h.Key, h.Value);
        }

        clone.Version = req.Version;

        foreach (KeyValuePair<string, object?> option in req.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        return clone;
    }

    private string ConvertExcelToString(byte[] excelFileData)
    {
        var sb = new StringBuilder();
        
        using var stream = new MemoryStream(excelFileData);
        using var workbook = new XSSFWorkbook(stream);
        
        for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
        {
            ISheet sheet = workbook.GetSheetAt(sheetIndex);
            sb.AppendLine($"--- Sheet: {sheet.SheetName} ---");
            
            ProcessSheetRows(sheet, sb);
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    private void ProcessSheetRows(ISheet sheet, StringBuilder sb)
    {
        for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            IRow? row = sheet.GetRow(rowIndex);
            if (row == null) continue;
            
            var rowValues = ExtractRowValues(row);
            sb.AppendLine(string.Join("\t", rowValues));
        }
    }

    private List<string> ExtractRowValues(IRow row)
    {
        var values = new List<string>();
        
        for (int cellIndex = 0; cellIndex < row.LastCellNum; cellIndex++)
        {
            ICell? cell = row.GetCell(cellIndex);
            values.Add(cell?.ToString() ?? string.Empty);
        }
        
        return values;
    }
    
    private CreateMealPlanRequest DeserializeAndValidateMealPlan(
        string mealPlanJson, string fileName, Guid dieticianId, Guid clientId)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        
        var mealPlanRequest = JsonSerializer.Deserialize<CreateMealPlanRequest>(mealPlanJson, options);
        
        if (mealPlanRequest == null)
        {
            throw new InvalidOperationException("Failed to parse meal plan data from OpenRouter API response");
        }
        
        mealPlanRequest.DieticianId = dieticianId;
        mealPlanRequest.ClientId = clientId;
        
        if (string.IsNullOrEmpty(mealPlanRequest.Title))
        {
            mealPlanRequest.Title = Path.GetFileNameWithoutExtension(fileName);
        }
        
        if (mealPlanRequest.StartDate == default)
        {
            mealPlanRequest.StartDate = DateTime.UtcNow.Date;
        }
        
        if (mealPlanRequest.EndDate == default)
        {
            mealPlanRequest.EndDate = mealPlanRequest.StartDate.AddDays(7);
        }
        
        if (mealPlanRequest.Days.Count == 0)
        {
            throw new InvalidOperationException("No meal days found in the Excel data");
        }
        
        return mealPlanRequest;
    }
}