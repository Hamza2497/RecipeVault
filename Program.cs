using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipeVault.Data;
using RecipeVault.Models;
using RecipeVault.Repositories;
using RecipeVault.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container - this registers MVC/Controller support
builder.Services.AddControllers();

// Register the Entity Framework database context with SQLite.
// This tells the app: "Use SQLite with the connection string named 'DefaultConnection'".
// The using statements at the top import DbContextServiceCollectionExtensions which provides AddDbContext.
// We'll get the connection string from appsettings.json later.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the Recipe Repository for dependency injection.
// This tells ASP.NET: "Whenever a controller asks for IRecipeRepository, give it a RecipeRepository instance".
// AddScoped means a new instance is created per HTTP request, which is ideal for database operations.
// The repository uses the registered AppDbContext internally.
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();

// Register the Authentication Service for dependency injection.
// This tells ASP.NET: "Whenever a controller asks for IAuthService, give it an AuthService instance".
// The service handles user registration, login, password hashing, and JWT token generation.
builder.Services.AddScoped<IAuthService, AuthService>();

// Register the Gemini AI Service for dependency injection.
// This tells ASP.NET: "Whenever a controller asks for IGeminiService, give it a GeminiService instance".
// The service handles all communication with the Gemini API for recipe generation and modification.
// AddScoped means a new instance is created per HTTP request.
builder.Services.AddScoped<IGeminiService, GeminiService>();

// Register HttpClient for dependency injection.
// HttpClient is used by GeminiService to make API calls to the Gemini API.
// AddHttpClient() registers it as a transient service (new instance each time it's injected).
// Transient is appropriate for HttpClient since it's thread-safe and stateless.
builder.Services.AddHttpClient();

// Configure JWT (JSON Web Token) authentication.
// This tells ASP.NET how to validate JWT tokens sent by clients.
// When a client includes "Authorization: Bearer {token}" in a request,
// ASP.NET will validate the token using these settings.
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
string secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey not configured in appsettings.json");

// Convert the secret string to bytes for the security algorithm
var key = Encoding.UTF8.GetBytes(secretKey);

// AddAuthentication() registers authentication services (the foundation)
// AddJwtBearer() configures JWT specifically
builder.Services.AddAuthentication(options =>
{
    // Set JWT as the default authentication scheme
    // This means: "When we need to check if a request is authenticated, use JWT"
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate the signing key (is it signed with our secret?)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        // Validate the issuer (who created the token?)
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],

        // Validate the audience (who is this token for?)
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],

        // Validate the expiration time (is the token still valid?)
        ValidateLifetime = true,

        // Allow a 5-second clock skew in case server clocks are slightly out of sync
        ClockSkew = TimeSpan.FromSeconds(5)
    };
});

// Configure CORS (Cross-Origin Resource Sharing) to allow requests from the React frontend
// This is necessary because the React app runs on a different origin (localhost:5173)
// than the API (localhost:5159), so browsers block the requests by default.
// CORS policy tells the browser: "These cross-origin requests are allowed."
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// OpenAPI (Swagger) configuration for API documentation
builder.Services.AddOpenApi();

var app = builder.Build();

// Database Seeder: Initialize the database with test data on startup
// This code runs once when the application starts, before any HTTP requests are handled.
//
// Step 1: Create a scope - this gets a temporary container of services just for this operation.
// Scopes are important because they ensure database connections are properly cleaned up.
// Think of it like: "I need a temporary database session to do some setup work."
using (var scope = app.Services.CreateScope())
{
    // Step 2: Get the database context from the scope.
    // This is the same AppDbContext we registered in Program.cs with AddDbContext().
    // The service provider creates a new instance and injects it here automatically.
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Step 3: Check if any users already exist in the database.
    // If this is a fresh database, context.Users.Any() will return false.
    // We only seed data if the table is empty, so we don't create duplicate test users.
    if (!context.Users.Any())
    {
        // Step 4: Create a test user object with the specified values.
        // In a real app, the PasswordHash would be generated by a hashing function like bcrypt.
        // For testing purposes, we're using a placeholder string.
        var testUser = new User
        {
            Username = "testuser",
            Email = "test@recipevault.com",
            PasswordHash = "placeholder123",
            CreatedAt = DateTime.UtcNow
        };

        // Step 5: Add the user to the Users table and save to the database.
        // context.Users.Add() stages the user for insertion.
        // context.SaveChanges() executes the SQL INSERT statement and commits the transaction.
        context.Users.Add(testUser);
        context.SaveChanges();
    }

    // Database Seeder: Initialize the database with sample recipes on startup
    // This follows the same idempotent pattern as user seeding - only inserts if no recipes exist.
    if (!context.Recipes.Any())
    {
        var sampleRecipes = new List<Recipe>
        {
            new Recipe
            {
                Name = "Spicy Thai Basil Chicken",
                Description = "A vibrant and aromatic Thai stir-fry featuring succulent chicken with fresh basil and a spicy kick.",
                CuisineType = "Thai",
                PrepTimeMinutes = 10,
                CookTimeMinutes = 15,
                Servings = 2,
                Status = "favourite",
                IsPublic = true,
                ImageUrl = "https://images.unsplash.com/photo-1569050467447-ce54b3bbc37d?w=1080",
                UserId = 1,
                Ingredients = "500g chicken breast (diced), 3 cloves garlic (minced), 2 red chilies (sliced), 1 cup fresh Thai basil, 3 tbsp fish sauce, 2 tbsp vegetable oil, 1 tbsp lime juice, 2 tbsp palm sugar, 200ml coconut milk",
                Instructions = "Heat oil in a wok or large skillet over high heat.\nAdd minced garlic and sliced chilies, stir-fry for 30 seconds.\nAdd diced chicken and cook until golden, about 5-7 minutes.\nAdd fish sauce, palm sugar, and coconut milk. Simmer for 3-4 minutes.\nAdd fresh Thai basil leaves and lime juice, toss gently.\nServe hot with jasmine rice."
            },
            new Recipe
            {
                Name = "Classic Margherita Pizza",
                Description = "A timeless Italian favorite with fresh mozzarella, ripe tomatoes, basil, and olive oil on a crispy crust.",
                CuisineType = "Italian",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 25,
                Servings = 4,
                Status = "made-before",
                IsPublic = true,
                ImageUrl = "https://images.unsplash.com/photo-1604068549290-dea0e4a305ca?w=1080",
                UserId = 1,
                Ingredients = "500g pizza dough, 400g canned tomatoes, 300g fresh mozzarella, 30g fresh basil, 3 cloves garlic, 60ml olive oil, salt to taste, black pepper to taste",
                Instructions = "Preheat oven to 220°C (425°F).\nPress pizza dough onto a baking sheet or pizza stone.\nCrush canned tomatoes and spread over the dough, leaving a 1cm border.\nMinced garlic can be scattered over the sauce.\nDrizzle with olive oil and season with salt and pepper.\nBake for 15-18 minutes until the crust is golden.\nRemove from oven and tear fresh mozzarella over the top.\nAdd fresh basil leaves, drizzle with more olive oil if desired.\nServe hot."
            },
            new Recipe
            {
                Name = "Creamy Mushroom Risotto",
                Description = "A luxurious and creamy Italian rice dish loaded with tender mushrooms and finished with Parmesan cheese.",
                CuisineType = "Italian",
                PrepTimeMinutes = 10,
                CookTimeMinutes = 35,
                Servings = 4,
                Status = "to-try",
                IsPublic = true,
                ImageUrl = "https://images.unsplash.com/photo-1476124369491-e7addf5db371?w=1080",
                UserId = 1,
                Ingredients = "300g Arborio rice, 400g mixed mushrooms (sliced), 1 liter vegetable or chicken stock (warm), 1 onion (finely diced), 3 cloves garlic (minced), 150ml dry white wine, 100g Parmesan cheese (grated), 50g butter, 3 tbsp olive oil, salt to taste, black pepper to taste, fresh parsley for garnish",
                Instructions = "Heat olive oil in a large pan and sauté mushrooms until golden. Set aside.\nIn the same pan, heat butter and olive oil. Add diced onion and cook until soft, about 3 minutes.\nAdd minced garlic and cook for 1 minute.\nAdd Arborio rice and stir for 2-3 minutes to coat with oil.\nPour in white wine and stir until absorbed.\nAdd warm stock one ladle at a time, stirring constantly and waiting for each ladle to be absorbed before adding the next (about 18-20 minutes total).\nAdd mushrooms back in for the last 5 minutes of cooking.\nRemove from heat and stir in Parmesan cheese and remaining butter.\nSeason with salt and pepper. Garnish with fresh parsley and serve immediately."
            },
            new Recipe
            {
                Name = "Chicken Caesar Salad",
                Description = "A hearty and satisfying salad with grilled chicken, crisp romaine lettuce, and a classic creamy Caesar dressing.",
                CuisineType = "American",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 10,
                Servings = 2,
                Status = "favourite",
                IsPublic = true,
                ImageUrl = "https://images.unsplash.com/photo-1550304943-4f24f54ddde9?w=1080",
                UserId = 1,
                Ingredients = "350g chicken breast, 2 romaine lettuce heads (chopped), 50g Parmesan cheese (grated), 100g croutons, 3 cloves garlic (minced), 2 tbsp mayonnaise, 1 tbsp Dijon mustard, 2 tbsp lemon juice, 1 tbsp Worcestershire sauce, 2 anchovy fillets (optional), 3 tbsp olive oil, salt and pepper to taste",
                Instructions = "Season chicken breast with salt and pepper.\nHeat 2 tbsp olive oil in a skillet over medium-high heat and cook chicken for 5-6 minutes per side until cooked through.\nLet chicken cool for a few minutes, then slice.\nPrepare Caesar dressing: blend or mix minced garlic, mayonnaise, Dijon mustard, lemon juice, Worcestershire sauce, anchovies (if using), and 1 tbsp olive oil. Season with salt and pepper.\nToss chopped romaine lettuce with Caesar dressing.\nTop with sliced chicken, grated Parmesan cheese, and croutons.\nServe immediately."
            }
        };

        context.Recipes.AddRange(sampleRecipes);
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable HTTPS redirection for security
app.UseHttpsRedirection();

// Use CORS middleware - must come BEFORE authentication and authorization
// This applies the CORS policy we defined above and tells browsers to allow the requests
app.UseCors("AllowReactApp");

// Add authentication middleware to the pipeline.
// This must come BEFORE UseAuthorization().
// Authentication identifies who the user is (validates JWT token).
app.UseAuthentication();

// Add authorization middleware to the pipeline.
// This must come AFTER UseAuthentication().
// Authorization checks if the authenticated user has permission (checks [Authorize] attributes).
app.UseAuthorization();

// Map controller routes - this tells the app to use controllers we create
app.MapControllers();

app.Run();
