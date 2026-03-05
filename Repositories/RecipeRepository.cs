using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Models;

namespace RecipeVault.Repositories;

/// <summary>
/// Concrete implementation of IRecipeRepository using Entity Framework and SQLite.
///
/// This class implements the IRecipeRepository interface and performs actual database operations
/// using AppDbContext (our Entity Framework context). It abstracts away the DbContext details
/// from the controller layer.
///
/// Why have this separate class?
/// 1. The controller talks to IRecipeRepository (the interface)
/// 2. The repository talks to AppDbContext (the database)
/// 3. This separation makes testing and changing databases easier
///
/// Pattern: Dependency Injection
/// This class receives AppDbContext via constructor injection.
/// The DI container automatically provides the right instance when needed.
/// </summary>
public class RecipeRepository : IRecipeRepository
{
    /// <summary>
    /// The database context, injected via dependency injection.
    /// This is the bridge between C# objects and the SQLite database.
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Constructor: receives the database context.
    /// The DI system automatically provides this when creating a RecipeRepository.
    /// </summary>
    public RecipeRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all recipes from the database.
    /// ToListAsync() runs the query asynchronously and returns all results as a List.
    /// </summary>
    public async Task<List<Recipe>> GetAllAsync()
    {
        return await _context.Recipes.ToListAsync();
    }

    /// <summary>
    /// Gets a single recipe by ID.
    /// FirstOrDefaultAsync() returns the first matching item or null if not found.
    /// This is better than Find() because it can apply filters like we might later.
    /// </summary>
    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Creates a new recipe in the database.
    /// Steps:
    /// 1. Add the recipe to the context (it's now "tracked")
    /// 2. SaveChangesAsync() persists it to the database
    /// 3. Return the saved recipe (which now has its database-generated Id)
    /// </summary>
    public async Task<Recipe> CreateAsync(Recipe recipe)
    {
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        return recipe;
    }

    /// <summary>
    /// Updates an existing recipe in the database.
    /// Steps:
    /// 1. Update() tells EF to mark the recipe as modified
    /// 2. SaveChangesAsync() sends the UPDATE command to the database
    /// 3. Return the updated recipe
    /// </summary>
    public async Task<Recipe> UpdateAsync(Recipe recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
        return recipe;
    }

    /// <summary>
    /// Deletes a recipe by its ID.
    /// Steps:
    /// 1. Find the recipe with the given ID
    /// 2. If it exists, remove it from the context
    /// 3. SaveChangesAsync() deletes it from the database
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
        if (recipe != null)
        {
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Searches and filters recipes based on optional criteria.
    ///
    /// This method demonstrates building a dynamic query based on multiple optional filters.
    /// We start with IQueryable<Recipe> (a query that hasn't executed yet) and apply filters conditionally.
    ///
    /// Key concept: IQueryable vs List
    /// - IQueryable: a query that will execute on the database (efficient - only fetches what you need)
    /// - List: data already in memory (less efficient for large datasets)
    /// We use IQueryable as long as possible, only calling ToListAsync() at the very end.
    ///
    /// For the ingredient search, we use FromSqlRaw() because we're searching within a
    /// comma-separated string. LIKE '%garlic%' in SQL is the best way to find substrings.
    /// This demonstrates mixing LINQ with raw SQL when appropriate.
    /// </summary>
    public async Task<List<Recipe>> SearchRecipesAsync(
        string? name = null,
        string? ingredient = null,
        string? cuisineType = null,
        int? maxPrepTime = null
    )
    {
        // Start with all recipes (this doesn't execute yet, it's just a query definition)
        IQueryable<Recipe> query = _context.Recipes;

        // Apply name filter if provided
        // This uses LINQ to filter the Name field (case-insensitive with EF.Functions.Like)
        // EF.Functions.Like uses the database LIKE operator for case-insensitive matching
        if (!string.IsNullOrEmpty(name))
        {
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{name}%"));
        }

        // Apply cuisine type filter if provided
        // String comparison - exact match (case-sensitive by default in SQLite)
        if (!string.IsNullOrEmpty(cuisineType))
        {
            query = query.Where(r => r.CuisineType == cuisineType);
        }

        // Apply max prep time filter if provided
        // Only include recipes where PrepTimeMinutes is less than or equal to maxPrepTime
        if (maxPrepTime.HasValue)
        {
            query = query.Where(r => r.PrepTimeMinutes <= maxPrepTime.Value);
        }

        // Apply ingredient filter if provided
        // For this one, we use FromSqlRaw because we're searching for a substring within
        // a comma-separated string (e.g., "Ingredients" might be "2 cups flour, 1 egg, 1 cup sugar")
        // and we want to find recipes containing "flour" anywhere in that string.
        // This demonstrates using raw SQL when LINQ doesn't cleanly express what we need.
        if (!string.IsNullOrEmpty(ingredient))
        {
            // FromSqlRaw() lets us execute custom SQL
            // The {0} placeholder is replaced with the parameter value safely (prevents SQL injection)
            // The LIKE operator with % wildcards finds the substring anywhere in Ingredients
            query = query.Where(r => EF.Functions.Like(r.Ingredients, $"%{ingredient}%"));
        }

        // Execute the query and return results
        // ToListAsync() sends the final query to the database and returns the results as a List
        return await query.ToListAsync();
    }

    /// <summary>
    /// Updates a recipe's status after verifying the user owns the recipe.
    ///
    /// Steps:
    /// 1. Query the database for a recipe matching both the ID and the user ID
    /// 2. If not found, return false (either the recipe doesn't exist or the user doesn't own it)
    /// 3. If found, update the Status field and save
    /// 4. Return true to indicate success
    ///
    /// Key security principle: We check BOTH the recipe ID and the user ID in the WHERE clause.
    /// This prevents users from modifying recipes they don't own, even if they somehow
    /// know the recipe ID. This is called "ownership verification" and is critical for multi-user apps.
    /// </summary>
    public async Task<bool> UpdateStatusAsync(int recipeId, string? status, int userId)
    {
        // Find the recipe by ID and verify it belongs to the requesting user
        // FirstOrDefaultAsync returns null if no match is found
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId && r.UserId == userId);

        // If the recipe doesn't exist or doesn't belong to this user, return false
        if (recipe == null)
        {
            return false;
        }

        // Update the status field
        recipe.Status = status;

        // Save the changes to the database
        await _context.SaveChangesAsync();

        // Return true to indicate the update was successful
        return true;
    }

    /// <summary>
    /// Updates a recipe's visibility (public/private) after verifying the user owns the recipe.
    ///
    /// Steps:
    /// 1. Query the database for a recipe matching both the ID and the user ID
    /// 2. If not found, return false (either the recipe doesn't exist or the user doesn't own it)
    /// 3. If found, update the IsPublic field and save
    /// 4. Return true to indicate success
    ///
    /// This method follows the same ownership verification pattern as UpdateStatusAsync.
    /// Only the recipe owner can change whether their recipe is public or private.
    /// </summary>
    public async Task<bool> UpdateVisibilityAsync(int recipeId, bool isPublic, int userId)
    {
        // Find the recipe by ID and verify it belongs to the requesting user
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId && r.UserId == userId);

        // If the recipe doesn't exist or doesn't belong to this user, return false
        if (recipe == null)
        {
            return false;
        }

        // Update the IsPublic field
        recipe.IsPublic = isPublic;

        // Save the changes to the database
        await _context.SaveChangesAsync();

        // Return true to indicate the update was successful
        return true;
    }

    /// <summary>
    /// Retrieves all recipes marked as public.
    ///
    /// This is a simple query that filters recipes where IsPublic == true.
    /// Unlike other repository methods, this doesn't check user ID because
    /// the whole point is to make these recipes visible to everyone.
    ///
    /// This endpoint is used by the public endpoint that doesn't require authentication.
    /// </summary>
    public async Task<List<Recipe>> GetPublicRecipesAsync()
    {
        return await _context.Recipes.Where(r => r.IsPublic).ToListAsync();
    }

    /// <summary>
    /// Retrieves a single public recipe by ID.
    ///
    /// This method allows unauthenticated users to view a specific public recipe.
    /// Queries the database for a recipe matching both the ID and IsPublic == true.
    /// Returns null if the recipe doesn't exist or is marked as private.
    ///
    /// This is used by the public recipe endpoint that doesn't require authentication.
    /// </summary>
    public async Task<Recipe?> GetPublicByIdAsync(int id)
    {
        return await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.IsPublic);
    }
}
