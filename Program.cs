var builder = WebApplication.CreateBuilder(args);

// Add services to the container - this registers MVC/Controller support
builder.Services.AddControllers();

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
