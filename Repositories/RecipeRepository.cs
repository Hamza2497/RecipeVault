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
}
