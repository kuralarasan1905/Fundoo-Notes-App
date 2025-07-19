using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Reminders.Commands.CreateReminder;
using fundoo_notes.Application.Features.Reminders.Commands.UpdateReminder;
using fundoo_notes.Application.Features.Reminders.Commands.DeleteReminder;
using fundoo_notes.Application.Features.Reminders.Commands.CompleteReminder;
using fundoo_notes.Application.Features.Reminders.Queries.GetUserReminders;
using fundoo_notes.Application.Features.Reminders.Queries.GetNoteReminders;
using fundoo_notes.Application.Services;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Reminders controller for managing note reminders
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class RemindersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RemindersController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public RemindersController(IMediator mediator, ILogger<RemindersController> logger, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get user's upcoming reminders
        /// </summary>
        /// <param name="includeCompleted">Include completed reminders</param>
        /// <returns>List of reminders</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<NoteReminderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteReminderDto>>> GetUserReminders([FromQuery] bool includeCompleted = false)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetUserRemindersQuery
                {
                    UserId = userId,
                    IncludeCompleted = includeCompleted
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reminders for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to get reminders. Please try again.");
            }
        }

        /// <summary>
        /// Get reminders for a specific note
        /// </summary>
        /// <param name="noteId">Note ID</param>
        /// <returns>List of note reminders</returns>
        [HttpGet("note/{noteId}")]
        [ProducesResponseType(typeof(List<NoteReminderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteReminderDto>>> GetNoteReminders(int noteId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetNoteRemindersQuery
                {
                    NoteId = noteId,
                    UserId = userId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {noteId} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reminders for note {NoteId} by user: {UserId}", noteId, GetCurrentUserId());
                return BadRequest("Failed to get note reminders. Please try again.");
            }
        }

        /// <summary>
        /// Create a new reminder
        /// </summary>
        /// <param name="request">Reminder creation data</param>
        /// <returns>Created reminder</returns>
        [HttpPost("CreateReminder")]
        [ProducesResponseType(typeof(NoteReminderDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteReminderDto>> CreateReminder([FromBody] CreateNoteReminderDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new CreateReminderCommand
                {
                    NoteId = request.NoteId,
                    ReminderDateTime = request.ReminderDateTime,
                    ReminderType = request.ReminderType,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetNoteReminders), new { noteId = request.NoteId }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reminder for note {NoteId} by user: {UserId}", request.NoteId, GetCurrentUserId());
                return BadRequest("Failed to create reminder. Please try again.");
            }
        }

        /// <summary>
        /// Update an existing reminder
        /// </summary>
        /// <param name="id">Reminder ID</param>
        /// <param name="request">Reminder update data</param>
        /// <returns>Updated reminder</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(NoteReminderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteReminderDto>> UpdateReminder(int id, [FromBody] UpdateNoteReminderDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new UpdateReminderCommand
                {
                    Id = id,
                    ReminderDateTime = request.ReminderDateTime,
                    ReminderType = request.ReminderType,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Reminder with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reminder {ReminderId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to update reminder. Please try again.");
            }
        }

        /// <summary>
        /// Mark reminder as completed
        /// </summary>
        /// <param name="id">Reminder ID</param>
        /// <returns>Updated reminder</returns>
        [HttpPatch("{id}/complete")]
        [ProducesResponseType(typeof(NoteReminderDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteReminderDto>> CompleteReminder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new CompleteReminderCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Reminder with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing reminder {ReminderId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to complete reminder. Please try again.");
            }
        }

        /// <summary>
        /// Delete a reminder
        /// </summary>
        /// <param name="id">Reminder ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteReminder(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new DeleteReminderCommand
                {
                    Id = id,
                    UserId = userId
                };

                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Reminder with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting reminder {ReminderId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to delete reminder. Please try again.");
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        /// <returns>User ID</returns>
        private int GetCurrentUserId()
        {
            return _currentUserService.GetCurrentUserIdAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Helper method to get user ID by email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User ID if found</returns>
        //private int? GetUserIdByEmail(string email)
        //{
        //    // Known user mapping for development
        //    var knownUsers = new Dictionary<string, int>
        //    {
        //        { "kuralarasan1905@gmail.com", 2 },
        //        { "admin@fundoonotes.com", 1 },
        //        { "test@example.com", 3 }
        //    };

        //    return knownUsers.TryGetValue(email.ToLowerInvariant(), out var userId) ? userId : null;
        //}
    }
}
