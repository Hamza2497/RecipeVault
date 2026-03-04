namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for returning recipe data to the client.
///
/// This DTO defines what fields the API sends back in response to GET requests.
/// It includes all recipe data that should be visible to users, including metadata
/// like Status, IsPublic, and CreatedAt.
///
/// The controller maps the internal Recipe model to this DTO before sending it to the client.
/// This maintains separation between the database model and the public API contract.
/// </summary>
public class RecipeResponseDto
{
    /// <summary>
    /// The unique identifier for this recipe.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The recipe name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The recipe description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The list of ingredients for this recipe.
    /// </summary>
    public string? Ingredients { get; set; }

    /// <summary>
    /// The cooking instructions for this recipe.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// The cuisine type (e.g., "Italian", "Asian").
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// Prep time in minutes.
    /// </summary>
    public int PrepTimeMinutes { get; set; }

    /// <summary>
    /// Cook time in minutes.
    /// </summary>
    public int CookTimeMinutes { get; set; }

    /// <summary>
    /// Number of servings this recipe makes.
    /// </summary>
    public int Servings { get; set; }

    /// <summary>
    /// URL to the recipe image, if available.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// The current status of this recipe (e.g., "favourite", "to try", "made before").
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Whether this recipe is publicly visible to other users.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// When this recipe was created, in UTC timezone.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
