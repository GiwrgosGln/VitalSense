using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using VitalSense.Application.DTOs;
using VitalSense.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace VitalSense.Application.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _geminiApiKey;
    private readonly string _geminiApiUrl;
    private readonly ILogger<GeminiService> _logger;

    private static class GeminiConstants
    {
        public const string JsonStartTag = "```json";
        public const string CodeBlockTag = "```";
        public const string ApiResponseCandidatesPath = "candidates";
        public const string ApiResponseContentPath = "content";
        public const string ApiResponsePartsPath = "parts";
        public const string ApiResponseTextField = "text";
    }

    public GeminiService(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _geminiApiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey not configured");
        _geminiApiUrl = configuration["Gemini:ApiUrl"] ?? "https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent";
        
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<CreateMealPlanRequest> ConvertExcelToMealPlanAsync(byte[] excelFileData, string fileName, Guid dieticianId, Guid clientId)
    {
        try
        {
            var excelContentAsText = ConvertExcelToString(excelFileData);

            var prompt = BuildGeminiPrompt(excelContentAsText, dieticianId, clientId);
            var requestJson = CreateGeminiRequestJson(prompt);
            var response = await SendGeminiRequest(requestJson);

            var mealPlanJson = ExtractJsonFromGeminiResponse(response);
            
            var mealPlanRequest = DeserializeAndValidateMealPlan(
                mealPlanJson, fileName, dieticianId, clientId);
            
            return mealPlanRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Excel file with Gemini API");
            throw new ApplicationException($"Error processing Excel file with Gemini API: {ex.Message}", ex);
        }
    }

    private string BuildGeminiPrompt(string excelContentAsText, Guid dieticianId, Guid clientId)
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

    private string CreateGeminiRequestJson(string prompt)
    {
        var geminiRequest = new
        {
            contents = new object[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                topK = 32,
                topP = 1.0,
                maxOutputTokens = 8192
            },
            safetySettings = new object[]
            {
                new
                {
                    category = "HARM_CATEGORY_HATE_SPEECH",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_DANGEROUS_CONTENT",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                    threshold = "BLOCK_NONE"
                },
                new
                {
                    category = "HARM_CATEGORY_HARASSMENT",
                    threshold = "BLOCK_NONE"
                }
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        return JsonSerializer.Serialize(geminiRequest, jsonOptions);
    }

    private async Task<string> SendGeminiRequest(string requestJson)
    {
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        var apiUrl = _geminiApiUrl;
        var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}?key={_geminiApiKey}");
        request.Content = requestContent;
        
        var response = await SendWithRetryAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API error: {StatusCode} - {ErrorContent}", 
                response.StatusCode, errorContent);
            throw new InvalidOperationException($"Gemini API returned status code {response.StatusCode}: {errorContent}");
        }
        
        return await response.Content.ReadAsStringAsync();
    }

    private string ExtractJsonFromGeminiResponse(string responseContent)
    {
        try
        {
            _logger.LogDebug("Processing response from Gemini API");
            
            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
            
            if (!responseJson.TryGetProperty(GeminiConstants.ApiResponseCandidatesPath, out var candidates) || 
                candidates.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Missing or empty candidates array in API response");
            }
            
            if (!candidates[0].TryGetProperty(GeminiConstants.ApiResponseContentPath, out var content))
            {
                throw new InvalidOperationException("Missing content object in API response");
            }
            
            if (!content.TryGetProperty(GeminiConstants.ApiResponsePartsPath, out var parts) ||
                parts.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Missing or empty parts array in API response");
            }
            
            if (!parts[0].TryGetProperty(GeminiConstants.ApiResponseTextField, out var text))
            {
                throw new InvalidOperationException("Missing text field in API response");
            }
            
            var jsonContent = text.GetString() ?? "";
            return CleanJsonResponse(jsonContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Failed to parse Gemini API response as JSON", ex);
        }
    }

    private string CleanJsonResponse(string jsonText)
    {
        var cleanedJson = jsonText.Trim();
        
        if (cleanedJson.StartsWith(GeminiConstants.JsonStartTag))
        {
            cleanedJson = cleanedJson[GeminiConstants.JsonStartTag.Length..];
        }
        else if (cleanedJson.StartsWith(GeminiConstants.CodeBlockTag))
        {
            cleanedJson = cleanedJson[GeminiConstants.CodeBlockTag.Length..];
        }
        
        if (cleanedJson.EndsWith(GeminiConstants.CodeBlockTag))
        {
            cleanedJson = cleanedJson[..^GeminiConstants.CodeBlockTag.Length];
        }
        
        return cleanedJson.Trim();
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
            throw new InvalidOperationException("Failed to parse meal plan data from Gemini API response");
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

                _logger.LogWarning("Rate limited by Gemini API. Retrying in {DelayMs}ms... (Attempt {Attempt}/{MaxRetries})", 
                    delayMilliseconds, attempts, maxRetries);
                await Task.Delay(delayMilliseconds);
                delayMilliseconds *= 2;
            }
            catch (Exception ex)
            {
                if (attempts >= maxRetries)
                {
                    throw new ApplicationException($"Failed to send request to Gemini API after {attempts} attempts.", ex);
                }
                _logger.LogWarning(ex, "Request to Gemini API failed. Retrying in {DelayMs}ms... (Attempt {Attempt}/{MaxRetries})",
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
}
