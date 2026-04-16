using CleanERP.CompositionRoot;
using CleanERP.Persistence.Contexts;
using CleanERP.Persistence.Seeders;
using CleanERP.API.Middleware;
using CleanERP.API.Extensions;
using CleanERP.API.Filters;
using Microsoft.EntityFrameworkCore;

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
