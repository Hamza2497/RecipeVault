using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container - this registers MVC/Controller support
builder.Services.AddControllers();

// Register the Entity Framework database context with SQLite.
// This tells the app: "Use SQLite with the connection string named 'DefaultConnection'".
// The using statements at the top import DbContextServiceCollectionExtensions which provides AddDbContext.
// We'll get the connection string from appsettings.json later.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// OpenAPI (Swagger) configuration for API documentation
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable HTTPS redirection for security
app.UseHttpsRedirection();

// Map controller routes - this tells the app to use controllers we create
app.MapControllers();

app.Run();
