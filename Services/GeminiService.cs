using System.Text;
using System.Text.Json;
using RecipeVault.DTOs;

namespace RecipeVault.Services;

/// <summary>
/// Implementation of IGeminiService that calls the Google Gemini API.
///
/// This service handles all communication with the Gemini API:
/// - Constructing detailed prompts based on user requests
/// - Calling the API via HTTP
/// - Parsing JSON responses from the API
/// - Converting API responses to domain DTOs
///
/// Key design decisions:
/// 1. Uses HttpClient for HTTP calls (thread-safe, reusable)
/// 2. Instructs the API to return ONLY JSON (no preamble, no markdown)
/// 3. Strips any markdown code fences that Gemini might include
/// 4. Reads configuration from appsettings.json via IConfiguration
/// 5. Throws exceptions for API errors (let the controller handle error responses)
/// </summary>
public class GeminiService : IGeminiService
{
    /// <summary>
    /// The HttpClient instance for making API calls.
    /// HttpClient is thread-safe and should be reused, not created per request.
    /// It's injected via dependency injection.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The Gemini API configuration read from appsettings.json.
    /// Contains ApiKey, Model, and ApiUrl settings.
    /// </summary>
    private readonly string _apiKey;
    private readonly string _apiUrl;

    /// <summary>
    /// Constructor: receives the injected HttpClient and configuration.
    ///
    /// IConfiguration is automatically available in ASP.NET Core and contains
    /// all settings from appsettings.json. We use GetSection("GeminiSettings")
    /// to access our specific configuration.
    /// </summary>
    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        // Read Gemini settings from appsettings.json
        var geminiSettings = configuration.GetSection("GeminiSettings");
        _apiKey = geminiSettings["ApiKey"] ?? throw new InvalidOperationException("Gemini ApiKey not configured in appsettings.json");
        _apiUrl = geminiSettings["ApiUrl"] ?? throw new InvalidOperationException("Gemini ApiUrl not configured in appsettings.json");
    }

    /// <summary>
    /// Generates a new recipe based on user constraints.
    ///
    /// This method constructs a detailed prompt that includes all constraints
    /// (prep time, cook time, allergies, dietary restrictions) and sends it to Gemini.
    /// The API returns a JSON recipe, which we parse and return as an AiRecipeResponseDto.
    ///
    /// The prompt is designed to:
    /// - Get ONLY JSON output (no extra text, no markdown)
    /// - Include all constraints in the recipe generation
    /// - Produce a consistent JSON structure we can parse
    /// </summary>
    public async Task<AiRecipeResponseDto> GenerateRecipeAsync(GenerateRecipeRequestDto request)
    {
        // Build the prompt with all user constraints
        var prompt = BuildGenerateRecipePrompt(request);

        // Call the Gemini API with our prompt
        var response = await CallGeminiAsync(prompt);

        // Log the raw JSON response for debugging purposes
        // This helps us see exactly what Gemini is returning
        Console.WriteLine("Gemini raw response: " + response);

        // Parse the JSON response into an AiRecipeResponseDto
        // Use case-insensitive property matching to handle different JSON casing from the API
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var recipe = JsonSerializer.Deserialize<AiRecipeResponseDto>(response, options)
            ?? throw new InvalidOperationException("Failed to parse recipe from Gemini response");

        return recipe;
    }

    /// <summary>
    /// Returns a list of ingredient substitutes.
    ///
    /// This method sends a focused prompt asking for 3 substitute options
    /// and returns them as a simple list of strings.
    /// </summary>
    public async Task<List<string>> GetIngredientSubstitutesAsync(string ingredientToReplace, string? reason = null)
    {
        // Build the prompt requesting substitutes
        var prompt = BuildSubstitutePrompt(ingredientToReplace, reason);

        // Call the Gemini API with our prompt
        var response = await CallGeminiAsync(prompt);

        // Log the raw JSON response for debugging purposes
        // This helps us see exactly what Gemini is returning
        Console.WriteLine("Gemini raw response: " + response);

        // Parse the JSON array response into a List<string>
        // Use case-insensitive property matching to handle different JSON casing from the API
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var substitutes = JsonSerializer.Deserialize<List<string>>(response, options)
            ?? throw new InvalidOperationException("Failed to parse substitutes from Gemini response");

        return substitutes;
    }

    /// <summary>
    /// Modifies an existing recipe based on a user request.
    ///
    /// This method sends the current recipe as JSON along with the tweak request
    /// and returns the modified recipe.
    /// </summary>
    public async Task<AiRecipeResponseDto> TweakRecipeAsync(AiRecipeResponseDto currentRecipe, string tweakRequest)
    {
        // Build the prompt with the current recipe and tweak request
        var prompt = BuildTweakRecipePrompt(currentRecipe, tweakRequest);

        // Call the Gemini API with our prompt
        var response = await CallGeminiAsync(prompt);

        // Log the raw JSON response for debugging purposes
        // This helps us see exactly what Gemini is returning
        Console.WriteLine("Gemini raw response: " + response);

        // Parse the JSON response into an AiRecipeResponseDto
        // Use case-insensitive property matching to handle different JSON casing from the API
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tweakedRecipe = JsonSerializer.Deserialize<AiRecipeResponseDto>(response, options)
            ?? throw new InvalidOperationException("Failed to parse tweaked recipe from Gemini response");

        return tweakedRecipe;
    }

    /// <summary>
    /// Calls the Gemini API with a prompt and returns the parsed response.
    ///
    /// This is the core method that:
    /// 1. Constructs the API request body
    /// 2. Makes the HTTP POST request to Gemini
    /// 3. Strips markdown code fences from the response (Gemini sometimes adds them)
    /// 4. Returns the cleaned response text
    ///
    /// The API expects the URL to include the API key as a query parameter.
    /// </summary>
    private async Task<string> CallGeminiAsync(string prompt)
    {
        // Construct the API request body
        // The Gemini API expects this specific JSON structure
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        // Serialize the request to JSON
        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        // Add the API key as a query parameter in the URL
        var urlWithKey = $"{_apiUrl}?key={_apiKey}";

        try
        {
            // Make the HTTP POST request to the Gemini API
            var response = await _httpClient.PostAsync(urlWithKey, jsonContent);

            // Throw an exception if the API returns an error status code
            response.EnsureSuccessStatusCode();

            // Read the response body as a string
            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse the API response JSON to extract the generated text
            var jsonResponse = JsonDocument.Parse(responseContent);
            var text = jsonResponse
                .RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
                ?? throw new InvalidOperationException("No text in Gemini response");

            // Strip markdown code fences if present
            // Gemini sometimes wraps JSON in ```json ... ``` code fences
            var cleanedText = StripMarkdownFences(text);

            return cleanedText;
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Error calling Gemini API: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Strips markdown code fences from text.
    ///
    /// Gemini sometimes wraps JSON responses in markdown code fences like:
    /// ```json
    /// { ... JSON ... }
    /// ```
    ///
    /// This method removes those fences if they exist.
    /// </summary>
    private static string StripMarkdownFences(string text)
    {
        // Remove ```json at the start and ``` at the end if present
        if (text.StartsWith("```json"))
        {
            text = text.Substring(7); // Remove "```json"
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3); // Remove "```"
        }

        if (text.EndsWith("```"))
        {
            text = text.Substring(0, text.Length - 3); // Remove trailing "```"
        }

        // Trim any remaining whitespace
        return text.Trim();
    }

    /// <summary>
    /// Builds the prompt for generating a recipe.
    ///
    /// This prompt is carefully constructed to:
    /// 1. Include all user constraints in a clear format
    /// 2. Ask for ONLY JSON output (no preamble, no explanation)
    /// 3. Specify the exact JSON structure we expect
    ///
    /// The constraints included are:
    /// - Recipe name (required)
    /// - Cuisine type (optional)
    /// - Max prep time (optional)
    /// - Max cook time (optional)
    /// - Allergies to avoid (optional)
    /// - Dietary restrictions (optional)
    /// </summary>
    private static string BuildGenerateRecipePrompt(GenerateRecipeRequestDto request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Generate a recipe for the following specifications:");
        sb.AppendLine($"Recipe: {request.RecipeName}");

        if (!string.IsNullOrEmpty(request.CuisineType))
            sb.AppendLine($"Cuisine Type: {request.CuisineType}");

        if (request.MaxPrepTimeMinutes.HasValue)
            sb.AppendLine($"Max Prep Time: {request.MaxPrepTimeMinutes} minutes");

        if (request.MaxCookTimeMinutes.HasValue)
            sb.AppendLine($"Max Cook Time: {request.MaxCookTimeMinutes} minutes");

        if (request.Allergies?.Count > 0)
            sb.AppendLine($"Avoid these allergens: {string.Join(", ", request.Allergies)}");

        if (request.DietaryRestrictions?.Count > 0)
            sb.AppendLine($"Follow these dietary restrictions: {string.Join(", ", request.DietaryRestrictions)}");

        sb.AppendLine();
        sb.AppendLine("Return ONLY valid JSON in this exact format, with no other text, no markdown, no explanation:");
        sb.AppendLine("{");
        sb.AppendLine("  \"name\": \"Recipe Name\",");
        sb.AppendLine("  \"ingredients\": [\"ingredient 1 with quantity\", \"ingredient 2 with quantity\"],");
        sb.AppendLine("  \"instructions\": [\"1. Step one\", \"2. Step two\"],");
        sb.AppendLine("  \"cuisineType\": \"Cuisine\",");
        sb.AppendLine("  \"prepTimeMinutes\": 15,");
        sb.AppendLine("  \"cookTimeMinutes\": 30,");
        sb.AppendLine("  \"servings\": 4");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Builds the prompt for getting ingredient substitutes.
    ///
    /// This prompt asks for 3 substitute options as a JSON array.
    /// If a reason is provided (e.g., "allergy"), it's included to help
    /// the AI choose appropriate substitutes.
    /// </summary>
    private static string BuildSubstitutePrompt(string ingredientToReplace, string? reason)
    {
        var sb = new StringBuilder();
        sb.Append($"Provide 3 substitute ingredients for '{ingredientToReplace}'");

        if (!string.IsNullOrEmpty(reason))
            sb.Append($" (reason: {reason})");

        sb.AppendLine(".");
        sb.AppendLine();
        sb.AppendLine("Return ONLY a JSON array of 3 strings with no other text or markdown:");
        sb.AppendLine("[\"substitute 1\", \"substitute 2\", \"substitute 3\"]");

        return sb.ToString();
    }

    /// <summary>
    /// Builds the prompt for tweaking a recipe.
    ///
    /// This prompt includes:
    /// 1. The current recipe as JSON
    /// 2. The user's tweak request in natural language
    /// 3. Instructions to return the modified recipe as JSON
    ///
    /// The tweak request allows flexible modifications like "make it spicier",
    /// "reduce cook time", "make it vegan", etc.
    /// </summary>
    private static string BuildTweakRecipePrompt(AiRecipeResponseDto currentRecipe, string tweakRequest)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You have the following recipe:");
        sb.AppendLine();
        sb.AppendLine(JsonSerializer.Serialize(currentRecipe, new JsonSerializerOptions { WriteIndented = true }));
        sb.AppendLine();
        sb.AppendLine($"Please modify it as follows: {tweakRequest}");
        sb.AppendLine();
        sb.AppendLine("Return ONLY the modified recipe as valid JSON in this exact format, with no other text, no markdown, no explanation:");
        sb.AppendLine("{");
        sb.AppendLine("  \"name\": \"Recipe Name\",");
        sb.AppendLine("  \"ingredients\": [\"ingredient 1 with quantity\", \"ingredient 2 with quantity\"],");
        sb.AppendLine("  \"instructions\": [\"1. Step one\", \"2. Step two\"],");
        sb.AppendLine("  \"cuisineType\": \"Cuisine\",");
        sb.AppendLine("  \"prepTimeMinutes\": 15,");
        sb.AppendLine("  \"cookTimeMinutes\": 30,");
        sb.AppendLine("  \"servings\": 4");
        sb.AppendLine("}");

        return sb.ToString();
    }
}
