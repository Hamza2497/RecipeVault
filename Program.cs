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
