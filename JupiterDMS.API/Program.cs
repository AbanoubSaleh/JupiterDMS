using System.Text;
using JupiterDMS.API.Middleware;
using JupiterDMS.Application;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Infrastructure.DataAccess;
using JupiterDMS.Infrastructure.DataAccess.Persistence;
using JupiterDMS.Infrastructure.Infra;
using JupiterDMS.Infrastructure.Infra.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = SerilogConfiguration.ConfigureLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of numbers
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "JupiterDMS API",
        Version = "v1",
        Description = "Enterprise Document Management System API",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "JupiterDMS Team"
        }
    });

    // Add JWT Bearer Authentication to Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Configure Swagger to handle enums properly
    options.UseInlineDefinitionsForEnums();

    // Add custom schema filters to handle complex types
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    // Configure to ignore circular references
    options.SupportNonNullableReferenceTypes();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

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

// Add health checks
builder.Services.AddHealthChecks();

// Add JWT Authentication
var jwtKey = builder.Configuration[DomainConstants.Jwt.KeyConfigurationKey];
var jwtIssuer = builder.Configuration[DomainConstants.Jwt.IssuerConfigurationKey];
var jwtAudience = builder.Configuration[DomainConstants.Jwt.AudienceConfigurationKey];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration is missing. Please check appsettings.json.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization with policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(DomainConstants.Auth.AdminPolicy, policy =>
        policy.RequireRole(DomainConstants.Auth.AdminRole));

    options.AddPolicy(DomainConstants.Auth.EditorPolicy, policy =>
        policy.RequireRole(DomainConstants.Auth.AdminRole, DomainConstants.Auth.EditorRole));

    options.AddPolicy(DomainConstants.Auth.ViewerPolicy, policy =>
        policy.RequireRole(DomainConstants.Auth.AdminRole, DomainConstants.Auth.EditorRole, DomainConstants.Auth.ViewerRole));
});

// Add application layers
builder.Services.AddApplication();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddInfra(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
// CORS must be before other middleware
app.UseCors("AllowAll");

// Enable Swagger in all environments (including production/deploy)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "JupiterDMS API v1");
});

// Note: HTTPS redirection is commented out for development
// Uncomment when using HTTPS in production
// app.UseHttpsRedirection();

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Migrate and seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JupiterDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DatabaseSeeder>>();

    try
    {
        // Automatically apply pending migrations
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");

        // Seed initial data
        var seeder = new DatabaseSeeder(context, logger);
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        // Log the error but don't fail the application startup
        // This allows the app to start even if database is not accessible during build/publish
        logger.LogError(ex, "An error occurred while migrating or seeding the database. The application will continue to start.");
    }
}

try
{
    Log.Information("Starting JupiterDMS API");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "JupiterDMS API terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

