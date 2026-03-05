namespace RecipeVault.DTOs;

public class GenerateImageRequestDto
{
    public required string RecipeName { get; set; }
    public string? CuisineType { get; set; }
}
