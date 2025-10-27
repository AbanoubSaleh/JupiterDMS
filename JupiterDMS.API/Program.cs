using System.Text;
using JupiterDMS.API.Middleware;
using JupiterDMS.Application;
using JupiterDMS.Domain.Constants;
using JupiterDMS.Infrastructure.DataAccess;
using JupiterDMS.Infrastructure.Infra;
using JupiterDMS.Infrastructure.Infra.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = SerilogConfiguration.ConfigureLogger();
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "JupiterDMS API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

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

