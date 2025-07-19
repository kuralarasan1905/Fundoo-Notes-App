using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using fundoo_notes.Domain.Interfaces;

namespace fundoo_notes.Infrastructure.Services
{
    /// <summary>
    /// Background service for processing note reminders
    /// </summary>
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

        public ReminderBackgroundService(
            ILogger<ReminderBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Reminder Background Service stopped");
        }

        private async Task ProcessReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var noteRepository = scope.ServiceProvider.GetRequiredService<INoteRepository>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var currentTime = DateTime.UtcNow;
            var reminderNotes = await noteRepository.GetNotesWithRemindersAsync(currentTime);

            foreach (var note in reminderNotes)
            {
                try
                {
                    if (note.ReminderDateTime.HasValue && 
                        note.ReminderDateTime.Value <= currentTime &&
                        note.User != null)
                    {
                        await emailService.SendReminderNotificationAsync(
                            note.User.Email,
                            note.Title,
                            note.ReminderDateTime.Value);

                        // Clear the reminder after sending notification
                        note.ReminderDateTime = null;
                        await noteRepository.UpdateAsync(note);
                        await noteRepository.SaveChangesAsync();

                        _logger.LogInformation("Reminder sent for note {NoteId} to user {UserId}", 
                            note.Id, note.UserId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process reminder for note {NoteId}", note.Id);
                }
            }
        }
    }
}
