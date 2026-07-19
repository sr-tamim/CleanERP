using CleanERP.CompositionRoot;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Seeders;
using CleanERP.API.Middleware;
using CleanERP.API.Extensions;
using CleanERP.API.Filters;
using CleanERP.Shared.Configuration;
using CleanERP.API.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers(options =>
{
    // Add the API logging filter globally
    options.Filters.Add<ApiLoggingFilter>();
});

// ===== COMPOSITION ROOT =====
// This is the ONLY place where all layers are wired together
// Following the Composition Root pattern for enterprise applications
builder.Services.AddCleanERP(builder.Configuration);

// Configure Authentication Settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<CookieSettings>(builder.Configuration.GetSection("CookieSettings"));
builder.Services.Configure<PasswordSettings>(builder.Configuration.GetSection("PasswordSettings"));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("SecuritySettings"));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null && !string.IsNullOrEmpty(jwtSettings.Secret))
{
    var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = false; // We're using HTTP-only cookies
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = jwtSettings.ClockSkew,
            RequireExpirationTime = true
        };
        
        // Configure cookie-based token extraction
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Try to get token from cookie first
                var cookieSettings = builder.Configuration.GetSection("CookieSettings").Get<CookieSettings>();
                if (cookieSettings != null)
                {
                    var token = context.Request.Cookies[cookieSettings.AccessTokenCookieName];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                }
                
                // Fallback to Authorization header if no cookie
                if (string.IsNullOrEmpty(context.Token))
                {
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                }
                
                return Task.CompletedTask;
            }
        };
    });
}

// Configure Authorization
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ModuleActionAuthorizationHandler>();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddModularSwagger();

// Add exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
await InitializeDatabaseAsync(app);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseModularSwaggerUI();
}

// Enable static files for custom CSS
app.UseStaticFiles();

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        logger.LogInformation("Database connection established successfully");
        
        // Seed sample data if needed
        await ApplicationDbContextSeed.SeedAsync(context);
        
        // Check final count
        var productCount = await context.Products.CountAsync();
        logger.LogInformation("Database contains {ProductCount} products", productCount);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        
        // Don't throw here to allow the app to start even if DB is not available
        // The health check endpoints will show the actual status
    }
}
