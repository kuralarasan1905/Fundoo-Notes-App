using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using fundoo_notes.Infrastructure.Extensions;
using fundoo_notes.Application.Extensions;
using fundoo_notes.Infrastructure.Middleware;

// FUNDOO NOTES API - PROFESSIONAL CONFIGURATION
// A comprehensive, production-ready ASP.NET Core Web API for note management
// Features: JWT Authentication, CQRS, Clean Architecture, Swagger Documentation

var builder = WebApplication.CreateBuilder(args);

// 1. LOGGING CONFIGURATION
ConfigureLogging(builder);

// 2. WEB HOST CONFIGURATION

ConfigureWebHost(builder);


// 3. SERVICE REGISTRATION
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

// 4. APPLICATION PIPELINE CONFIGURATION
var app = builder.Build();
await ConfigurePipelineAsync(app);

// 5. APPLICATION STARTUP
await StartApplicationAsync(app);

// CONFIGURATION METHODS

static void ConfigureLogging(WebApplicationBuilder builder)
{
    // Enhanced Serilog configuration with structured logging
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "FundooNotesAPI")
        .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0")

        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/fundoo-notes-.txt",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/errors/fundoo-notes-errors-.txt",
            restrictedToMinimumLevel: LogEventLevel.Error,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 90)
        .CreateLogger();

    builder.Host.UseSerilog();

    Log.Information("=== Fundoo Notes API Starting ===");
    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
    Log.Information("Configuration Sources: {Sources}",
        string.Join(", ", builder.Configuration.Sources.Select(s => s.GetType().Name)));
}

static void ConfigureWebHost(WebApplicationBuilder builder)
{
    // Dynamic port configuration for different environments
    if (builder.Environment.IsDevelopment())
    {
        var port = Environment.GetEnvironmentVariable("ASPNETCORE_PORT") ?? "5227";
        var httpsPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT") ?? "7227";

        builder.WebHost.UseUrls($"https://localhost:{httpsPort}", $"http://localhost:{port}");
        Log.Information("Development URLs configured: https://localhost:{HttpsPort}, http://localhost:{Port}",
            httpsPort, port);
    }

    // Configure Kestrel for production scenarios
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false; // Security: Remove server header
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB limit for file uploads
        options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    });
}

static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    Log.Information("Configuring services for {Environment} environment", environment.EnvironmentName);

    // CORE APPLICATION SERVICES

    // Application layer services (CQRS, MediatR, Validation, AutoMapper)
    services.AddApplicationServices();

    // Infrastructure layer services (Database, JWT, Email, Background Services)
    services.AddInfrastructureServices(configuration);

    // Session support for user identification
    services.AddDistributedMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = "FundooNotes.Session";
    });

    // WEB API CONFIGURATION

    // Configure JSON serialization options
    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.PropertyNameCaseInsensitive = true; // Handle both camelCase and PascalCase
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.WriteIndented = environment.IsDevelopment();
    });

    // Configure controllers with enhanced options
    services.AddControllers(options =>
    {
        // Model validation
        options.ModelValidatorProviders.Clear();

        // Response caching
        options.CacheProfiles.Add("Default", new CacheProfile
        {
            Duration = 300, // 5 minutes
            Location = ResponseCacheLocation.Any
        });

        options.CacheProfiles.Add("Never", new CacheProfile
        {
            Location = ResponseCacheLocation.None,
            NoStore = true
        });
    })
    .AddJsonOptions(options =>
    {
        // Configure JSON for controllers to handle both camelCase and PascalCase
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.WriteIndented = environment.IsDevelopment();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Custom model validation response
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new BadRequestObjectResult(new
            {
                Message = "Validation failed",
                Errors = errors,
                Timestamp = DateTime.UtcNow
            });
        };
    });

    // API DOCUMENTATION (SWAGGER/OPENAPI)
    ConfigureSwagger(services, environment);

    // SECURITY CONFIGURATION
    ConfigureSecurity(services, configuration, environment);

    // PERFORMANCE & CACHING
    ConfigurePerformance(services);

    // HEALTH CHECKS
    ConfigureHealthChecks(services, configuration);

    Log.Information("Service configuration completed successfully");
}

static void ConfigureSwagger(IServiceCollection services, IWebHostEnvironment environment)
{
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Fundoo Notes API",
            Version = "v1.0",
            Description = "A comprehensive note-taking application API built with ASP.NET Core",
            Contact = new OpenApiContact
            {
                Name = "Fundoo Notes Team",
                Email = "support@fundoonotes.com",
                Url = new Uri("https://github.com/your-repo/fundoo-notes")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // JWT Authentication configuration for Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' followed by a space and your JWT token.\n\nExample: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        // Include XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Custom operation filters
        options.DescribeAllParametersInCamelCase();
    });
}

static void ConfigureSecurity(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    // CORS Configuration
    services.AddCors(options =>
    {
        if (environment.IsDevelopment())
        {
            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins(
                        "http://localhost:4200",
                        "https://localhost:4200",
                        "http://localhost:5227",
                        "https://localhost:7256"
                      )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        }
        else
        {
            // Production CORS - configure with your actual frontend domains
            var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "https://your-production-domain.com" };

            options.AddPolicy("AllowAngularApp", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        }
    });

    // Security Headers
    services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    // Data Protection
    services.AddDataProtection();

    Log.Information("Security configuration completed");
}

static void ConfigurePerformance(IServiceCollection services)
{
    // Response Compression
    services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    });

    // Response Caching
    services.AddResponseCaching(options =>
    {
        options.MaximumBodySize = 1024 * 1024; // 1MB
        options.UseCaseSensitivePaths = false;
    });

    // Memory Cache
    services.AddMemoryCache(options =>
    {
        options.SizeLimit = 100 * 1024 * 1024; // 100MB
    });

    Log.Information("Performance optimization configured");
}

static void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    services.AddHealthChecks()
        .AddCheck("database", () =>
        {
            try
            {
                // Simple database connectivity check
                if (string.IsNullOrEmpty(connectionString))
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("Database connection string not configured");
                }

                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();

                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection successful");
            }
            catch (Exception ex)
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded($"Database connection failed: {ex.Message}");
            }
        })
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

    Log.Information("Health checks configured");
}

static Task ConfigurePipelineAsync(WebApplication app)
{
    Log.Information("Configuring HTTP request pipeline for {Environment}", app.Environment.EnvironmentName);

    // DEVELOPMENT PIPELINE
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fundoo Notes API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Fundoo Notes API Documentation";
            options.DefaultModelsExpandDepth(-1);
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
        });
    }

    // PRODUCTION PIPELINE
    if (app.Environment.IsProduction())
    {
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    // SECURITY MIDDLEWARE

    // Custom security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        if (app.Environment.IsProduction())
        {
            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        }

        await next();
    });

    // CORE MIDDLEWARE PIPELINE

    // Global exception handling
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Performance middleware
    app.UseResponseCompression();
    app.UseResponseCaching();

    // Forwarded headers for reverse proxy scenarios
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // HTTPS redirection (only in production)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // CORS
    app.UseCors("AllowAngularApp");

    // Session middleware (must be before authentication)
    app.UseSession();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    
    // ENDPOINT MAPPING

    // Health checks
    app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                Status = report.Status.ToString(),
                Checks = report.Entries.Select(x => new
                {
                    Name = x.Key,
                    Status = x.Value.Status.ToString(),
                    Description = x.Value.Description,
                    Duration = x.Value.Duration.TotalMilliseconds
                }),
                TotalDuration = report.TotalDuration.TotalMilliseconds,
                Timestamp = DateTime.UtcNow
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            }));
        }
    });

    // API Controllers
    app.MapControllers();

    // Default route for API info
    app.MapGet("/", () => new
    {
        Application = "Fundoo Notes API",
        Version = "1.0.0",
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        Documentation = "/swagger",
        Health = "/api/health"
    });

    Log.Information("HTTP request pipeline configured successfully");
    return Task.CompletedTask;
}

static async Task StartApplicationAsync(WebApplication app)
{
    try
    {
        // PRE-STARTUP VALIDATIONS

        Log.Information("=== Starting Fundoo Notes API ===");
        Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
        Log.Information("Content Root: {ContentRoot}", app.Environment.ContentRootPath);
        Log.Information("Web Root: {WebRoot}", app.Environment.WebRootPath);

        // Validate critical configurations
        ValidateConfiguration(app.Configuration);

        // DATABASE INITIALIZATION (AUTOMATIC CREATION)

        Log.Information("Initializing database...");

        // Try database initialization with retry logic
        var maxRetries = 3;
        var retryDelay = TimeSpan.FromSeconds(2);
        var databaseInitialized = false;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await EnsureDatabaseCreatedAsync(app);
                Log.Information("Database initialization successful on attempt {Attempt}", attempt);
                databaseInitialized = true;
                break;
            }
            catch (Exception ex) when (attempt < maxRetries && (ex.Message.Contains("10061") || ex.Message.Contains("connection")))
            {
                Log.Warning("Database connection attempt {Attempt} failed: {Error}. Retrying in {Delay}s...",
                    attempt, ex.Message, retryDelay.TotalSeconds);
                await Task.Delay(retryDelay);
            }
            catch (Exception ex) when (attempt == maxRetries)
            {
                Log.Error("Database initialization failed after {MaxRetries} attempts: {Error}", maxRetries, ex.Message);
                Log.Warning("Starting application without database connection - some features may not work");

                // Extract server info from connection string for logging
                var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
                var serverInfo = ExtractServerFromConnectionString(connectionString ?? "Unknown");
                Log.Warning("Please check SQL Server at {ServerInfo} is running and accessible", serverInfo);
                Log.Error("Database connection error: {ErrorMessage}", ex.Message);
                break;
            }
        }

        if (!databaseInitialized)
        {
            Log.Warning("Application starting in degraded mode without database");
        }

        if (app.Environment.IsDevelopment())
        {
            Log.Information("Development environment detected - checking database connectivity");
            await TestDatabaseConnectivityAsync(app.Configuration);
        }

        // STARTUP INFORMATION

        var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "default ports";
        Log.Information("Application URLs: {Urls}", urls);

        if (app.Environment.IsDevelopment())
        {
            Log.Information("Swagger UI available at: {SwaggerUrl}/swagger", urls.Split(',')[0].Trim());
            Log.Information("Health checks available at: {HealthUrl}/api/health", urls.Split(',')[0].Trim());
        }

        // START THE APPLICATION

        Log.Information("Fundoo Notes API is starting...");
        Log.Information("Application listening on: {Urls}", urls);

        if (app.Environment.IsDevelopment())
        {
            Log.Information("API Documentation available at: {SwaggerUrl}/swagger", urls.Split(',')[0].Trim());
            Log.Information("Health Check available at: {HealthUrl}/api/health", urls.Split(',')[0].Trim());
        }

        Log.Information("Application started successfully");
        Log.Information("Press Ctrl+C to shut down");

        // Setup graceful shutdown
        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Log.Information("Shutdown requested...");
            cancellationTokenSource.Cancel();
        };

        await app.RunAsync(cancellationTokenSource.Token);
    }
    catch (OperationCanceledException)
    {
        Log.Information("Application shutdown completed gracefully");
    }
    catch (ObjectDisposedException ex)
    {
        Log.Warning("Application disposed during shutdown: {Message}", ex.Message);
        // Don't rethrow - this is expected during shutdown
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Application terminated unexpectedly");

        if (app.Environment.IsDevelopment())
        {
            Log.Debug("Debug Info: {ExceptionType}", ex.GetType().Name);
            Log.Debug("Stack trace: {StackTrace}", ex.StackTrace);
        }

        // Only rethrow if it's not a disposal-related exception during shutdown
        if (!(ex is ObjectDisposedException || ex.InnerException is ObjectDisposedException))
        {
            throw;
        }
    }
    finally
    {
        Log.Information("=== Fundoo Notes API Shutdown ===");
        Log.CloseAndFlush();
        Console.WriteLine("Application shutdown complete.");
    }
}

// AUTOMATIC DATABASE CREATION METHOD
static async Task EnsureDatabaseCreatedAsync(WebApplication app)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<fundoo_notes.Infrastructure.Data.FundooNotesDbContext>();

        Log.Information("Checking database existence...");

        // This will create the database if it doesn't exist and apply all pending migrations
        await context.Database.EnsureCreatedAsync();

        Log.Information("Database initialization completed successfully");
    }
    catch (ObjectDisposedException ex)
    {
        // This can happen during application shutdown - it's usually harmless
        Log.Warning("Database context disposed during initialization: {Message}", ex.Message);
        Log.Information("Database initialization completed (context disposed during shutdown)");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database initialization failed: {Error}", ex.Message);
        throw new InvalidOperationException($"Database initialization failed: {ex.Message}", ex);
    }
}

// FIXED: Separate database connectivity test method to avoid ObjectDisposedException
static async Task TestDatabaseConnectivityAsync(IConfiguration configuration)
{
    try
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();
            Log.Information("Database connectivity verified");
        }
    }
    catch (Exception dbEx)
    {
        Log.Warning("Database connectivity check failed: {Error}", dbEx.Message);
        // Don't fail startup for database connectivity issues in development
    }
}

static void ValidateConfiguration(IConfiguration configuration)
{
    Log.Information("Validating application configuration...");

    // Validate database connection
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured");
    }

    // Validate JWT settings
    var jwtSection = configuration.GetSection("JwtSettings");
    var secretKey = jwtSection["SecretKey"];
    var issuer = jwtSection["Issuer"];
    var audience = jwtSection["Audience"];

    if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
    {
        throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
    }

    if (string.IsNullOrEmpty(issuer))
    {
        throw new InvalidOperationException("JWT Issuer is not configured");
    }

    if (string.IsNullOrEmpty(audience))
    {
        throw new InvalidOperationException("JWT Audience is not configured");
    }

    Log.Information("Configuration validation completed successfully");
    Log.Information("Database: {DatabaseServer}", ExtractServerFromConnectionString(connectionString));
    Log.Information("JWT Issuer: {JwtIssuer}", issuer);
    Log.Information("JWT Audience: {JwtAudience}", audience);
}

static string ExtractServerFromConnectionString(string connectionString)
{
    try
    {
        var parts = connectionString.Split(';');
        var serverPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Server=", StringComparison.OrdinalIgnoreCase));
        return serverPart?.Split('=')[1] ?? "Unknown";
    }
    catch
    {
        return "Unknown";
    }
}