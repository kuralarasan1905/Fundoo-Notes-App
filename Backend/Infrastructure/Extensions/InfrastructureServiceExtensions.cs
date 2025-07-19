using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using fundoo_notes.Infrastructure.Data;
using fundoo_notes.Infrastructure.Repositories;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Infrastructure.Services;
using fundoo_notes.Application.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace fundoo_notes.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for configuring infrastructure services
    /// </summary>
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database with optimized configuration
            services.AddDbContext<FundooNotesDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                {
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    sqlOptions.CommandTimeout(30);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                    sqlOptions.MigrationsAssembly("fundoo-notes");
                });

                // Performance optimizations
                options.EnableSensitiveDataLogging(false); // Disable in production
                options.EnableServiceProviderCaching();
                options.EnableDetailedErrors(false); // Disable in production
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.MultipleCollectionIncludeWarning));
            });

            // Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ILabelRepository, LabelRepository>();
            services.AddScoped<IAttachmentRepository, AttachmentRepository>();
            services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();
            services.AddScoped<INoteTemplateRepository, NoteTemplateRepository>();
            services.AddScoped<INoteHistoryRepository, NoteHistoryRepository>();

            // JWT Authentication
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("JWT SecretKey is not configured");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // HTTP Context Accessor
            services.AddHttpContextAccessor();

            // Services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ILoginHistoryService, LoginHistoryService>();
            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

            // Background Services
            services.AddHostedService<ReminderBackgroundService>();
            services.AddHostedService<CleanupBackgroundService>();

            // Caching Service
            services.AddSingleton<ICacheService, CacheService>();

            // Performance Monitoring Service
            services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();

            return services;
        }
    }
}
