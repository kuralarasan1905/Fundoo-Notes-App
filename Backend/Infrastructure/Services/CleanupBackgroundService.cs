using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Background service for cleanup tasks
    /// </summary>
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly ILogger<CleanupBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run daily
        private readonly TimeSpan _trashRetentionPeriod = TimeSpan.FromDays(7); // Keep trashed items for 7 days (like Google Keep)

        public CleanupBackgroundService(
            ILogger<CleanupBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanupTasks();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during cleanup tasks");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Cleanup Background Service stopped");
        }

        private async Task PerformCleanupTasks()
        {
            using var scope = _serviceProvider.CreateScope();
            var noteRepository = scope.ServiceProvider.GetRequiredService<INoteRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            await CleanupOldTrashedNotes(noteRepository);
            await CleanupExpiredTokens(userRepository);
            
            _logger.LogInformation("Cleanup tasks completed successfully");
        }

        private async Task CleanupOldTrashedNotes(INoteRepository noteRepository)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(_trashRetentionPeriod);
                var oldTrashedNotes = await noteRepository.FindAsync(n => 
                    n.IsTrashed && n.UpdatedAt < cutoffDate);

                if (oldTrashedNotes.Any())
                {
                    await noteRepository.DeleteRangeAsync(oldTrashedNotes);
                    await noteRepository.SaveChangesAsync();
                    
                    _logger.LogInformation("Permanently deleted {Count} old trashed notes", 
                        oldTrashedNotes.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old trashed notes");
            }
        }

        private async Task CleanupExpiredTokens(IUserRepository userRepository)
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var usersWithExpiredTokens = await userRepository.FindAsync(u => 
                    (u.EmailVerificationTokenExpiry.HasValue && u.EmailVerificationTokenExpiry < currentTime) ||
                    (u.PasswordResetTokenExpiry.HasValue && u.PasswordResetTokenExpiry < currentTime));

                foreach (var user in usersWithExpiredTokens)
                {
                    bool updated = false;

                    if (user.EmailVerificationTokenExpiry.HasValue && user.EmailVerificationTokenExpiry < currentTime)
                    {
                        user.EmailVerificationToken = null;
                        user.EmailVerificationTokenExpiry = null;
                        updated = true;
                    }

                    if (user.PasswordResetTokenExpiry.HasValue && user.PasswordResetTokenExpiry < currentTime)
                    {
                        user.PasswordResetToken = null;
                        user.PasswordResetTokenExpiry = null;
                        updated = true;
                    }

                    if (updated)
                    {
                        await userRepository.UpdateAsync(user);
                    }
                }

                if (usersWithExpiredTokens.Any())
                {
                    await userRepository.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up expired tokens for {Count} users", 
                        usersWithExpiredTokens.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired tokens");
            }
        }
    }
}
