using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using RecipeVault.Data;
using RecipeVault.DTOs;
using RecipeVault.Models;

namespace RecipeVault.Services;

/// <summary>
/// Service for handling user authentication (registration and login).
///
/// This service:
/// - Hashes passwords securely using BCrypt (never stores plain passwords)
/// - Generates JWT tokens for authenticated users
/// - Validates credentials during login
/// - Reads configuration from appsettings.json via IConfiguration
///
/// Why a service? Services encapsulate business logic, keeping controllers thin and focused.
/// This also makes it easier to test and reuse the authentication logic.
/// </summary>
public class AuthService : IAuthService
{
    // Dependency injection: these are provided by ASP.NET's DI container
    private readonly AppDbContext _context;        // Database context for CRUD operations
    private readonly IConfiguration _configuration; // Configuration (appsettings.json, secrets)

    /// <summary>
    /// Constructor: receives the database context and configuration via dependency injection.
    /// ASP.NET automatically provides these when AuthService is instantiated.
    /// </summary>
    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new user account.
    ///
    /// Steps:
    /// 1. Check if the email is already registered (prevent duplicates)
    /// 2. Hash the password using BCrypt (one-way encryption - can't reverse it)
    /// 3. Create a new User record in the database
    /// 4. Generate a JWT token so they're logged in immediately
    /// 5. Return the token and user info
    /// </summary>
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Step 1: Check if user with this email already exists
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email already registered");
        }

        // Step 2: Hash the plain-text password using BCrypt
        // BCrypt automatically handles salting (adding randomness to prevent rainbow table attacks)
        // This creates a unique hash even for identical passwords
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Step 3: Create a new User entity with the hashed password
        // IMPORTANT: We store the hash, NEVER the plain password
        var newUser = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

        // Add to database and commit the transaction
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Step 4: Generate a JWT token for immediate login
        string token = GenerateJwtToken(newUser);

        // Step 5: Return the response with token and user info
        return new AuthResponseDto
        {
            Token = token,
            Username = newUser.Username,
            Email = newUser.Email
        };
    }

    /// <summary>
    /// Authenticate a user and return a JWT token.
    ///
    /// Steps:
    /// 1. Find the user by email
    /// 2. Hash the provided password and compare with the stored hash
    /// 3. If valid, generate and return a JWT token
    /// 4. If invalid, throw an exception
    /// </summary>
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Step 1: Find the user by email
        // FirstOrDefault returns null if not found (doesn't throw)
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid email or password");
        }

        // Step 2: Verify the password
        // BCrypt.Verify() hashes the provided password and compares with the stored hash
        // This is safe because hashing is one-way - we can never decrypt the stored hash
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Invalid email or password");
        }

        // Step 3 & 4: Generate JWT token and return response
        string token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
            Username = user.Username,
            Email = user.Email
        };
    }

    /// <summary>
    /// Generate a JWT (JSON Web Token) for the authenticated user.
    ///
    /// How JWT works:
    /// 1. Create claims (user info): Id, Username, Email
    /// 2. Sign these claims with a secret key (only the server knows this key)
    /// 3. Encode as a token string that looks like: "header.payload.signature"
    ///
    /// The client includes this token in API requests: Authorization: Bearer {token}
    /// The server validates the signature to ensure the token wasn't tampered with.
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        // Read JWT settings from appsettings.json
        // The "JwtSettings:" prefix tells ASP.NET to look under the JwtSettings section
        var jwtSettings = _configuration.GetSection("JwtSettings");
        string secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        string issuer = jwtSettings["Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer not configured");
        string audience = jwtSettings["Audience"]
            ?? throw new InvalidOperationException("JWT Audience not configured");

        // Parse the expiry hours from config (default to 24 if not set)
        int expiryHours = int.TryParse(jwtSettings["ExpiryHours"], out var hours) ? hours : 24;

        // Step 1: Create claims - these are pieces of information about the user
        // The server will extract these from the token later to know who made the request
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID
            new Claim(ClaimTypes.Name, user.Username),                 // Username
            new Claim(ClaimTypes.Email, user.Email)                    // Email
        };

        // Step 2: Create a signing key
        // The key must be at least 256 bits (32 bytes) for HS256 algorithm
        // We convert the secret string to bytes using UTF-8 encoding
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        // Step 3: Create credentials for signing
        // This tells the JWT library to sign with HMAC SHA256
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Step 4: Create the token descriptor with all the metadata
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),           // The claims (user info)
            Expires = DateTime.UtcNow.AddHours(expiryHours), // When the token expires
            Issuer = issuer,                                 // Who created this token
            Audience = audience,                             // Who can use this token
            SigningCredentials = credentials                 // How to sign it
        };

        // Step 5: Create and write the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        // Convert the token to a string (the format the client will send back)
        string token = tokenHandler.WriteToken(securityToken);

        return token;
    }
}
