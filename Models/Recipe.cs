namespace RecipeVault.Models;

/// <summary>
/// Represents a recipe in the RecipeVault application.
///
/// Each recipe belongs to a user and contains cooking instructions, ingredients, and metadata.
/// The Status field tracks whether the user has made it, wants to try it, or marked it as a favourite.
/// </summary>
public class Recipe
{
    /// <summary>
    /// Primary key - uniquely identifies this recipe in the database.
    /// Entity Framework automatically recognizes properties named "Id" as primary keys.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The recipe name (e.g., "Chocolate Chip Cookies"). Required - recipes must have a name.
    /// The "required" modifier ensures this field cannot be null.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// A text description or summary of the recipe. Optional - not all recipes need a description.
    /// Nullable string - can be null or empty.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// A comma-separated list of ingredients (e.g., "2 cups flour, 1 cup sugar, 1 egg").
    /// In a production app, you'd have a separate Ingredients table instead of a string.
    /// This is a simplified approach for initial setup.
    /// </summary>
    public string? Ingredients { get; set; }

    /// <summary>
    /// Step-by-step cooking instructions as a text field.
    /// In a production app, you might store this as formatted text or structured steps.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// The type of cuisine (e.g., "Italian", "Asian", "Mexican").
    /// This helps users filter and organize recipes by cuisine type.
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// Time in minutes to prepare ingredients before cooking.
    /// Helps users plan their cooking time.
    /// </summary>
    public int PrepTimeMinutes { get; set; }

    /// <summary>
    /// Time in minutes to actively cook the recipe.
    /// Combined with PrepTime, gives total time to complete the recipe.
    /// </summary>
    public int CookTimeMinutes { get; set; }

    /// <summary>
    /// How many people this recipe serves.
    /// Users may want to scale recipes up or down based on serving size.
    /// </summary>
    public int Servings { get; set; }

    /// <summary>
    /// URL to an image of the finished dish.
    /// Allows the app to display a photo with the recipe.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Current status of this recipe in the user's collection.
    /// Possible values: "favourite", "to try", "made before", or null for no status.
    /// This helps users categorize and find recipes they care about.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Determines if this recipe is publicly visible to other users.
    /// Defaults to false - recipes are private by default for privacy.
    /// When set to true, other users can view and potentially save it.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Timestamp when this recipe was added to the database.
    /// Automatically set to UTC now when the recipe is created.
    /// Useful for sorting recipes by "newest first".
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Foreign key: the ID of the user who owns this recipe.
    /// Links this recipe to a specific user in the User table.
    /// Every recipe must have an owner - this field cannot be null.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Navigation property: a reference to the User object that owns this recipe.
    /// This allows you to write code like: var owner = recipe.User.Username;
    /// Entity Framework manages this relationship - you don't manually populate it.
    /// </summary>
    public User? User { get; set; }
}
