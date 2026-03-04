using RecipeVault.Models;

namespace RecipeVault.Repositories;

/// <summary>
/// Repository interface for Recipe data access operations.
///
/// An interface defines a "contract" - it specifies what methods must exist
/// without implementing them. The RecipeRepository class will implement this interface.
///
/// Why use interfaces?
/// 1. Abstraction: Controllers depend on the interface, not the concrete class
/// 2. Testability: You can create fake/mock repositories for testing
/// 3. Flexibility: You can swap implementations (e.g., swap SQLite for PostgreSQL)
///    without changing the controller code
///
/// This is the "Repository Pattern" - a common design pattern for data access.
/// The repository acts as an "in-memory collection" abstraction over the database.
/// </summary>
public interface IRecipeRepository
{
    /// <summary>
    /// Retrieves all recipes from the database.
    /// Async methods are marked with Task/Task&lt;T&gt; and should be awaited.
    /// </summary>
    /// <returns>A collection of all recipes.</returns>
    Task<List<Recipe>> GetAllAsync();

    /// <summary>
    /// Retrieves a single recipe by its ID.
    /// Returns null if no recipe with that ID exists.
    /// </summary>
    /// <param name="id">The recipe ID to look up.</param>
    /// <returns>The recipe if found; null otherwise.</returns>
    Task<Recipe?> GetByIdAsync(int id);

    /// <summary>
    /// Creates and saves a new recipe to the database.
    /// The recipe object is prepared by the caller (the controller).
    /// </summary>
    /// <param name="recipe">The Recipe object to save. Must have Name and UserId set.</param>
    /// <returns>The saved recipe (may include generated fields like Id and CreatedAt).</returns>
    Task<Recipe> CreateAsync(Recipe recipe);

    /// <summary>
    /// Updates an existing recipe in the database.
    /// The recipe object should have the Id set to identify which record to update.
    /// </summary>
    /// <param name="recipe">The Recipe object with updated values.</param>
    /// <returns>The updated recipe.</returns>
    Task<Recipe> UpdateAsync(Recipe recipe);

    /// <summary>
    /// Deletes a recipe from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the recipe to delete.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Searches for recipes based on optional filter parameters.
    ///
    /// This method demonstrates flexible filtering - each parameter is optional.
    /// If a parameter is null or empty, that filter is skipped.
    /// This allows callers to use any combination of filters.
    ///
    /// For example:
    /// - SearchAsync("Pasta", null, null, null) finds recipes by name only
    /// - SearchAsync(null, "garlic", "Italian", null) combines ingredient and cuisine filters
    /// - SearchAsync(null, null, null, 30) finds recipes with prep time ≤ 30 minutes
    ///
    /// Why optional parameters? Flexibility. Different searches need different filters,
    /// and having one method handle all combinations is cleaner than multiple overloads.
    /// </summary>
    /// <param name="name">Optional: Search for recipe name (case-insensitive partial match)</param>
    /// <param name="ingredient">Optional: Search for ingredient substring in the ingredients list</param>
    /// <param name="cuisineType">Optional: Filter by exact cuisine type match</param>
    /// <param name="maxPrepTime">Optional: Filter for recipes with prep time ≤ this value (in minutes)</param>
    /// <returns>A list of recipes matching the provided filters.</returns>
    Task<List<Recipe>> SearchRecipesAsync(
        string? name = null,
        string? ingredient = null,
        string? cuisineType = null,
        int? maxPrepTime = null
    );

    /// <summary>
    /// Updates a recipe's status with ownership verification.
    ///
    /// This method finds a recipe by ID and verifies it belongs to the requesting user
    /// before updating the Status field. This ensures users can only modify their own recipes.
    ///
    /// The Status field should be one of: "favourite", "to-try", "made-before", or null to clear.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to update</param>
    /// <param name="status">The new status value (should be "favourite", "to-try", "made-before", or null)</param>
    /// <param name="userId">The ID of the requesting user (for ownership verification)</param>
    /// <returns>True if the recipe was updated successfully; false if not found or user doesn't own it</returns>
    Task<bool> UpdateStatusAsync(int recipeId, string? status, int userId);

    /// <summary>
    /// Updates a recipe's visibility (public/private) with ownership verification.
    ///
    /// This method finds a recipe by ID and verifies it belongs to the requesting user
    /// before updating the IsPublic field. This ensures users can only change visibility
    /// on their own recipes.
    ///
    /// When IsPublic is true, the recipe will appear in the public recipes endpoint.
    /// When IsPublic is false, only the owner can see the recipe.
    /// </summary>
    /// <param name="recipeId">The ID of the recipe to update</param>
    /// <param name="isPublic">Whether the recipe should be publicly visible (true) or private (false)</param>
    /// <param name="userId">The ID of the requesting user (for ownership verification)</param>
    /// <returns>True if the recipe was updated successfully; false if not found or user doesn't own it</returns>
    Task<bool> UpdateVisibilityAsync(int recipeId, bool isPublic, int userId);

    /// <summary>
    /// Retrieves all recipes that are marked as public.
    ///
    /// This endpoint is accessible to all users (authenticated or not) and returns
    /// a list of recipes that the recipe owners have shared publicly.
    ///
    /// This allows recipe discovery and sharing without requiring user authentication.
    /// </summary>
    /// <returns>A list of all recipes where IsPublic == true</returns>
    Task<List<Recipe>> GetPublicRecipesAsync();
}
