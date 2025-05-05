using Microsoft.EntityFrameworkCore;
using VesselManagementApi.Data;
using VesselManagementApi.Interfaces;
using VesselManagementApi.Services;
using VesselManagementApi.Mapping;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// Add DbContext for Entity Framework Core
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // defined in appsttings.json
if (string.IsNullOrEmpty(connectionString)) {
    connectionString = "Server=.\\SQLEXPRESS;Database=VesselManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";
    Console.WriteLine("DefaultConnection not found in appsettings.json. Using default LocalDB connection string.");
}

builder.Services.AddDbContext<VesselManagementDbContext>(options => options.UseSqlServer(connectionString));

// Add Interfaces
builder.Services.AddScoped<IOwnerInterface, OwnerInterface>();
builder.Services.AddScoped<IShipInterface, ShipInterface>();

// Add Services
builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IShipService, ShipService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Add controllers service
builder.Services.AddControllers();


// Add Swagger/OpenAPI generation services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Vessel Management API",
        Description = "A RESTful Vessel Management System built with C#, ASP.NET Core, Entity Framework, and SQL Server, featuring CRUD operations and a many-to-many relationship between ship owners and vessels.",
        Contact = new OpenApiContact
        {
            Name = "Neagu Andrei"
        }
    });
});

// Build the app
var app = builder.Build();

// Enable Swagger and Swagger UI (only in dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vessel Management API V1");
    });
    // Apply migrations automatically on startup (only in dev)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<VesselManagementDbContext>();
        try
        {
            Console.WriteLine("Applying database migrations (Development)...");
            dbContext.Database.Migrate();
            Console.WriteLine("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying migrations: {ex.Message}");
        }
    }

}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Add authorization middleware
app.UseAuthorization();

// Map controller routes
app.MapControllers();

app.Run();