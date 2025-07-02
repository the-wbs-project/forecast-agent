using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using WeatherGuard.Api.Filters;
using WeatherGuard.Api.Middleware;
using WeatherGuard.Api.Configuration;
using WeatherGuard.Api.Services;
using WeatherGuard.Core.Interfaces;
using WeatherGuard.Infrastructure.Data;
using WeatherGuard.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/weatherguard-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add Database Context
builder.Services.AddDbContext<WeatherGuardDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repository and Unit of Work
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Datadog Configuration and Service
builder.Services.AddSingleton<IDatadogConfig>(provider => 
    new DatadogConfig(builder.Configuration as IConfigurationRoot ?? throw new InvalidOperationException("Configuration not available")));
builder.Services.AddSingleton<DatadogService>();

// Add Services
builder.Services.AddScoped<IProjectService, ProjectService>();
// TODO: Implement these services
// builder.Services.AddScoped<IUserService, UserService>();
// builder.Services.AddScoped<ITaskService, TaskService>();
// builder.Services.AddScoped<IWeatherRiskAnalysisService, WeatherRiskAnalysisService>();
// builder.Services.AddScoped<IProjectScheduleService, ProjectScheduleService>();
// builder.Services.AddScoped<IAuthService, AuthService>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured - please set in appsettings.Development.json")))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Controllers with validation filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// TODO: Add API versioning (requires Microsoft.AspNetCore.Mvc.Versioning package)
// builder.Services.AddApiVersioning(opt =>
// {
//     opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
//     opt.AssumeDefaultVersionWhenUnspecified = true;
//     opt.ReadVersionFromHeader = true;
//     opt.HeaderNames.Add("X-Version");
// });

// builder.Services.AddVersionedApiExplorer(setup =>
// {
//     setup.GroupNameFormat = "'v'VVV";
//     setup.SubstituteApiVersionInUrl = true;
// });

// Add Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "WeatherGuard API", 
        Version = "v1",
        Description = "API for managing construction projects with weather risk analysis",
        Contact = new OpenApiContact
        {
            Name = "WeatherGuard Support",
            Email = "support@weatherguard.com"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Add Health Checks
builder.Services.AddHealthChecks();
    // TODO: Add database health check (requires Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore package)
    // .AddDbContextCheck<WeatherGuardDbContext>("database");

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add HTTP Client for external APIs
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<DatadogLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherGuard API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<WeatherGuardDbContext>();
    if (app.Environment.IsDevelopment())
    {
        context.Database.EnsureCreated();
    }
}

app.Run();

public partial class Program { }
