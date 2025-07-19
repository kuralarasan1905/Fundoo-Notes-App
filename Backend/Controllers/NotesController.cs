using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Common.Helpers;
using fundoo_notes.Application.Features.Notes.Commands.CreateNote;
using fundoo_notes.Application.Features.Notes.Commands.UpdateNote;
using fundoo_notes.Application.Features.Notes.Commands.ToggleNotePin;
using fundoo_notes.Application.Features.Notes.Commands.ToggleNoteArchive;
using fundoo_notes.Application.Features.Notes.Commands.DeleteNote;
using fundoo_notes.Application.Features.Notes.Commands.MoveNoteToTrash;
using fundoo_notes.Application.Features.Notes.Commands.ToggleNoteTrash;
using fundoo_notes.Application.Features.Notes.Commands.BulkPinNotes;
using fundoo_notes.Application.Features.Notes.Commands.PinUnpinNote;
using fundoo_notes.Application.Features.Notes.Queries.GetUserNotes;
using fundoo_notes.Application.Features.Notes.Queries.GetNoteById;
using fundoo_notes.Application.Features.Notes.Queries.SearchNotes;
using fundoo_notes.Application.Features.Notes.Queries.GetNotesByLabel;
using fundoo_notes.Application.Features.Templates.Commands.CreateNoteFromTemplate;
using fundoo_notes.Application.Features.Notes.Queries.GetNoteHistory;
using fundoo_notes.Application.Features.Notes.Commands.AddLabelToNote;
using fundoo_notes.Application.Features.Notes.Commands.RemoveLabelFromNote;
using fundoo_notes.Application.Features.Notes.Commands.BulkAddLabel;
using fundoo_notes.Application.Features.Notes.Commands.BulkRemoveLabel;
using fundoo_notes.Application.Features.Notes.Commands.ManageNoteLabels;
using fundoo_notes.Application.Features.Notes.Commands.RestoreNote;
using fundoo_notes.Application.Features.Notes.Commands.BulkRestoreNotes;
using fundoo_notes.Domain.Interfaces;
using fundoo_notes.Application.Services;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Notes controller for managing user notes
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class NotesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NotesController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public NotesController(IMediator mediator, ILogger<NotesController> logger, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Test endpoint to verify authentication bypass is working
        /// </summary>
        /// <returns>Test response</returns>
        [HttpGet("test")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public ActionResult<object> TestEndpoint()
        {
            var userId = GetCurrentUserId();
            return Ok(new {
                message = "Authentication bypass is working!",
                userId = userId,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get current user's notes
        /// </summary>
        /// <param name="includeArchived">Include archived notes</param>
        /// <param name="includeTrashed">Include trashed notes</param>
        /// <returns>List of user notes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> GetNotes(
            [FromQuery] bool includeArchived = false,
            [FromQuery] bool includeTrashed = false)
        {
            var userId = GetCurrentUserId();

            var query = new GetUserNotesQuery
            {
                UserId = userId,
                IncludeArchived = includeArchived,
                IncludeTrashed = includeTrashed
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get current user's notes list (alias for frontend compatibility)
        /// </summary>
        /// <returns>List of user notes</returns>
        [HttpGet("getNotesList")]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> GetNotesList()
        {
            var userId = GetCurrentUserId();

            var query = new GetUserNotesQuery
            {
                UserId = userId,
                IncludeArchived = false,
                IncludeTrashed = false
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Get archived notes list
        /// </summary>
        /// <returns>List of archived notes</returns>
        [HttpGet("getArchiveNotesList")]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> GetArchiveNotesList()
        {
            var userId = GetCurrentUserId();

            var query = new GetUserNotesQuery
            {
                UserId = userId,
                IncludeArchived = true,
                IncludeTrashed = false
            };

            var result = await _mediator.Send(query);
            // Filter to only archived notes
            var archivedNotes = result.Where(n => n.IsArchived).ToList();
            return Ok(archivedNotes);
        }

        /// <summary>
        /// Get trashed notes list
        /// </summary>
        /// <returns>List of trashed notes</returns>
        [HttpGet("getTrashNotesList")]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> GetTrashNotesList()
        {
            var userId = GetCurrentUserId();

            var query = new GetUserNotesQuery
            {
                UserId = userId,
                IncludeArchived = false,
                IncludeTrashed = true
            };

            var result = await _mediator.Send(query);
            // Filter to only trashed notes
            var trashedNotes = result.Where(n => n.IsTrashed).ToList();
            return Ok(trashedNotes);
        }

        /// <summary>
        /// Get reminder notes list
        /// </summary>
        /// <returns>List of notes with reminders</returns>
        [HttpGet("getReminderNotesList")]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> GetReminderNotesList()
        {
            var userId = GetCurrentUserId();

            var query = new GetUserNotesQuery
            {
                UserId = userId,
                IncludeArchived = false,
                IncludeTrashed = false
            };

            var result = await _mediator.Send(query);
            // Filter to only notes with reminders
            var reminderNotes = result.Where(n => n.HasReminder).ToList();
            return Ok(reminderNotes);
        }

        /// <summary>
        /// Get notes filtered by label
        /// </summary>
        /// <param name="labelId">Label ID to filter by</param>
        /// <returns>List of notes with the specified label</returns>
        [HttpGet("byLabel/{labelId}")]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> GetNotesByLabel(int labelId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetNotesByLabelQuery
                {
                    LabelId = labelId,
                    UserId = userId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notes by label {LabelId} for user: {UserId}", labelId, GetCurrentUserId());
                return BadRequest("Failed to get notes by label. Please try again.");
            }
        }

        /// <summary>
        /// Search notes by title and content
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="includeArchived">Include archived notes</param>
        /// <param name="includeTrashed">Include trashed notes</param>
        /// <returns>List of matching notes</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<NoteListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteListDto>>> SearchNotes(
            [FromQuery] string searchTerm,
            [FromQuery] bool includeArchived = false,
            [FromQuery] bool includeTrashed = false)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term cannot be empty");
            }

            if (searchTerm.Length < 2)
            {
                return BadRequest("Search term must be at least 2 characters long");
            }

            try
            {
                var userId = GetCurrentUserId();

                var query = new SearchNotesQuery
                {
                    SearchTerm = searchTerm.Trim(),
                    UserId = userId,
                    IncludeArchived = includeArchived,
                    IncludeTrashed = includeTrashed
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching notes for user: {UserId} with term: {SearchTerm}", GetCurrentUserId(), searchTerm);
                return BadRequest("Failed to search notes. Please try again.");
            }
        }

        /// <summary>
        /// Create a new note
        /// </summary>
        /// <param name="request">Note creation data</param>
        /// <returns>Created note</returns>
        [HttpPost("addNotes")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> CreateNote([FromBody] CreateNoteDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var command = new CreateNoteCommand
                {
                    Title = request.Title,
                    Content = request.Content,
                    Color = request.Color,
                    ReminderDateTime = request.ReminderDateTime,
                    LabelIds = request.LabelIds,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetNoteById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to create note. Please try again.");
            }
        }

        /// <summary>
        /// Get note by ID
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Note details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> GetNoteById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetNoteByIdQuery
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound($"Note with ID {id} not found");
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting note {NoteId} for user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to retrieve note. Please try again.");
            }
        }

        /// <summary>
        /// Update an existing note
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <param name="request">Note update data</param>
        /// <returns>Updated note</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> UpdateNote(int id, [FromBody] UpdateNoteDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new UpdateNoteCommand
                {
                    Id = id,
                    Title = request.Title,
                    Content = request.Content,
                    Color = request.Color,
                    ReminderDateTime = request.ReminderDateTime,
                    LabelIds = request.LabelIds, // This will be null if not provided, preserving existing labels
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note {NoteId} for user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to update note. Please try again.");
            }
        }

        /// <summary>
        /// Update notes (bulk operation)
        /// </summary>
        /// <param name="request">Update notes request</param>
        /// <returns>Updated notes</returns>
        [HttpPost("updateNotes")]
        [ProducesResponseType(typeof(List<NoteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteDto>>> UpdateNotes([FromBody] BulkUpdateNotesDto request)
        {
            if (request.Notes == null || !request.Notes.Any())
            {
                return BadRequest("Notes list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedNotes = new List<NoteDto>();

                foreach (var noteUpdate in request.Notes)
                {
                    try
                    {
                        var command = new UpdateNoteCommand
                        {
                            Id = noteUpdate.Id,
                            Title = noteUpdate.Title,
                            Content = noteUpdate.Content,
                            Color = noteUpdate.Color,
                            ReminderDateTime = noteUpdate.ReminderDateTime,
                            LabelIds = noteUpdate.LabelIds, // This will be null if not provided, preserving existing labels
                            UserId = userId
                        };

                        var result = await _mediator.Send(command);
                        updatedNotes.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to update note {NoteId}", noteUpdate.Id);
                    }
                }

                return Ok(updatedNotes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to update notes. Please try again.");
            }
        }

        /// <summary>
        /// Toggle note pin status
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Updated note</returns>
        [HttpPatch("{id}/pin")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> TogglePin(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new ToggleNotePinCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling pin for note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to toggle pin status. Please try again.");
            }
        }

        /// <summary>
        /// Toggle note archive status
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Updated note</returns>
        [HttpPatch("{id}/archive")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> ToggleArchive(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new ToggleNoteArchiveCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling archive for note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to toggle archive status. Please try again.");
            }
        }

        /// <summary>
        /// Move note to trash (soft delete) - for main interface delete button
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}/trash")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> MoveNoteToTrash(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new MoveNoteToTrashCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                return Ok(new { message = "Note moved to trash successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving note {NoteId} to trash by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to move note to trash. Please try again.");
            }
        }

        /// <summary>
        /// Toggle note trash status (move to/from bin) - for restore functionality
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Updated note</returns>
        [HttpPatch("{id}/trash")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> ToggleTrash(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new ToggleNoteTrashCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling trash for note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to toggle trash status. Please try again.");
            }
        }

        /// <summary>
        /// Restore a note from trash
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Restored note</returns>
        [HttpPatch("{id}/restore")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NoteDto>> RestoreNote(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new RestoreNoteCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
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
                _logger.LogError(ex, "Error restoring note {NoteId} for user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to restore note. Please try again.");
            }
        }

        /// <summary>
        /// Delete a note permanently (hard delete)
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteNote(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new DeleteNoteCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to delete note. Please try again.");
            }
        }

        /// <summary>
        /// Get available colors and patterns for notes
        /// </summary>
        /// <returns>Available colors and patterns</returns>
        [HttpGet("colors")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // Cache for 1 hour
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetAvailableColors()
        {
            return Ok(new
            {
                colors = ColorHelper.ValidColors.ToList(),
                patterns = ColorHelper.ValidPatterns.ToList(),
                message = "Available colors (hex codes) and patterns (identifiers) for notes"
            });
        }

        /// <summary>
        /// Update note color
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <param name="color">New color (hex code like #FF0000 or pattern like 'grid', 'lines', 'dots', 'gradient')</param>
        /// <returns>Updated note</returns>
        [HttpPatch("{id}/color")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<NoteDto>> UpdateNoteColor(int id, [FromBody] string color)
        {
            try
            {
                // Validate color format first
                if (!ColorHelper.IsValidColor(color))
                {
                    return BadRequest(new
                    {
                        error = "Invalid color format",
                        message = $"Color '{color}' is not valid. Use hex colors (e.g., #FF0000) or patterns (grid, lines, dots, gradient)",
                        validColors = ColorHelper.ValidColors.ToList(),
                        validPatterns = ColorHelper.ValidPatterns.ToList()
                    });
                }

                var userId = GetCurrentUserId();

                // Get the current note
                var noteQuery = new GetNoteByIdQuery
                {
                    Id = id,
                    UserId = userId
                };

                var currentNote = await _mediator.Send(noteQuery);
                if (currentNote == null)
                {
                    return NotFound($"Note with ID {id} not found");
                }

                // Update with new color
                var command = new UpdateNoteCommand
                {
                    Id = id,
                    Title = currentNote.Title,
                    Content = currentNote.Content,
                    Color = color,
                    ReminderDateTime = currentNote.ReminderDateTime,
                    LabelIds = currentNote.Labels?.Where(l => l != null).Select(l => l.Id).ToList() ?? [],
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    error = "Invalid color format",
                    message = ex.Message,
                    validColors = ColorHelper.ValidColors.ToList(),
                    validPatterns = ColorHelper.ValidPatterns.ToList()
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating color for note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to update note color. Please try again.");
            }
        }

        /// <summary>
        /// Change color of notes (bulk operation)
        /// </summary>
        /// <param name="request">Color change request</param>
        /// <returns>Success status</returns>
        [HttpPost("changesColorNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangeNotesColor([FromBody] BulkColorChangeDto request)
        {
            if (request.NoteIdList == null || !request.NoteIdList.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedNotes = new List<NoteDto>();
                var noteIds = request.GetNoteIds();

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        // Get the current note
                        var noteQuery = new GetNoteByIdQuery
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        var currentNote = await _mediator.Send(noteQuery);
                        if (currentNote != null)
                        {
                            // Update with new color
                            var command = new UpdateNoteCommand
                            {
                                Id = noteId,
                                Title = currentNote.Title,
                                Content = currentNote.Content,
                                Color = request.Color,
                                ReminderDateTime = currentNote.ReminderDateTime,
                                LabelIds = currentNote.Labels?.Where(l => l != null).Select(l => l.Id).ToList() ?? new List<int>(),
                                UserId = userId
                            };

                            var result = await _mediator.Send(command);
                            updatedNotes.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to change color for note {NoteId}", noteId);
                    }
                }

                return Ok(new { message = $"Color changed for {updatedNotes.Count} notes", updatedNotes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing notes color for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to change notes color. Please try again.");
            }
        }

        /// <summary>
        /// Archive/Unarchive notes (bulk operation)
        /// </summary>
        /// <param name="request">Archive request</param>
        /// <returns>Success status</returns>
        [HttpPost("archiveNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ArchiveNotes([FromBody] BulkArchiveDto request)
        {
            if (request.NoteIdList == null || !request.NoteIdList.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedNotes = new List<NoteDto>();
                var noteIds = request.GetNoteIds();

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        // Get the current note
                        var noteQuery = new GetNoteByIdQuery
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        var currentNote = await _mediator.Send(noteQuery);
                        if (currentNote != null)
                        {
                            // Use the note repository to set archive status
                            var noteRepository = HttpContext.RequestServices.GetRequiredService<INoteRepository>();
                            await noteRepository.ArchiveNoteAsync(noteId, request.IsArchived);

                            // Get the updated note
                            var updatedNote = await _mediator.Send(noteQuery);
                            if (updatedNote != null)
                            {
                                updatedNotes.Add(updatedNote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to archive note {NoteId}", noteId);
                    }
                }

                return Ok(new { message = $"Archive status updated for {updatedNotes.Count} notes", updatedNotes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to archive notes. Please try again.");
            }
        }

        /// <summary>
        /// Trash/Untrash notes (bulk operation)
        /// </summary>
        /// <param name="request">Trash request</param>
        /// <returns>Success status</returns>
        [HttpPost("trashNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> TrashNotes([FromBody] BulkTrashDto request)
        {
            if (request.NoteIdList == null || !request.NoteIdList.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedNotes = new List<NoteDto>();
                var noteIds = request.GetNoteIds();

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        // Get the current note
                        var noteQuery = new GetNoteByIdQuery
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        var currentNote = await _mediator.Send(noteQuery);
                        if (currentNote != null)
                        {
                            // Use the note repository to set trash status
                            var noteRepository = HttpContext.RequestServices.GetRequiredService<INoteRepository>();
                            await noteRepository.TrashNoteAsync(noteId, request.IsDeleted);

                            // Get the updated note
                            var updatedNote = await _mediator.Send(noteQuery);
                            if (updatedNote != null)
                            {
                                updatedNotes.Add(updatedNote);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to trash note {NoteId}", noteId);
                    }
                }

                return Ok(new { message = $"Trash status updated for {updatedNotes.Count} notes", updatedNotes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trashing notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to trash notes. Please try again.");
            }
        }

        /// <summary>
        /// Delete notes forever (bulk operation)
        /// </summary>
        /// <param name="request">Delete forever request</param>
        /// <returns>Success status</returns>
        [HttpPost("deleteForeverNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteForeverNotes([FromBody] BulkDeleteDto request)
        {
            if (request.NoteIdList == null || !request.NoteIdList.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var deletedCount = 0;
                var noteIds = request.GetNoteIds();

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        var command = new DeleteNoteCommand
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        await _mediator.Send(command);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete note {NoteId}", noteId);
                    }
                }

                return Ok(new { message = $"{deletedCount} notes deleted permanently" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notes forever for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to delete notes. Please try again.");
            }
        }

        /// <summary>
        /// Restore notes from trash (bulk operation)
        /// </summary>
        /// <param name="request">Restore request</param>
        /// <returns>Restore result</returns>
        [HttpPost("restoreNotes")]
        [ProducesResponseType(typeof(BulkRestoreNotesResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BulkRestoreNotesResult>> RestoreNotes([FromBody] BulkTrashDto request)
        {
            if (request.NoteIdList == null || !request.NoteIdList.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var noteIds = request.GetNoteIds();

                var command = new BulkRestoreNotesCommand
                {
                    NoteIds = noteIds,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to restore notes. Please try again.");
            }
        }

        /// <summary>
        /// Permanently delete note from trash - for trash page delete button
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}/permanent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> PermanentlyDeleteNote(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Get the note first to verify it exists, user has access, and it's in trash
                var noteQuery = new GetNoteByIdQuery
                {
                    Id = id,
                    UserId = userId
                };

                var currentNote = await _mediator.Send(noteQuery);
                if (currentNote == null)
                {
                    return NotFound($"Note with ID {id} not found");
                }

                // Only allow permanent deletion if note is in trash
                if (!currentNote.IsTrashed)
                {
                    return BadRequest("Note must be in trash before permanent deletion");
                }

                // Permanently delete the note
                var command = new DeleteNoteCommand
                {
                    Id = id,
                    UserId = userId
                };

                await _mediator.Send(command);

                _logger.LogInformation("Note {NoteId} permanently deleted from trash by user: {UserId}", id, userId);
                return Ok(new { message = "Note permanently deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
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
                _logger.LogError(ex, "Error permanently deleting note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to permanently delete note. Please try again.");
            }
        }

        /// <summary>
        /// Bulk permanently delete notes from trash - for trash page bulk delete
        /// </summary>
        /// <param name="noteIds">List of note IDs to permanently delete</param>
        /// <returns>Success status with results</returns>
        [HttpDelete("bulk/permanent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> BulkPermanentlyDeleteNotes([FromBody] List<int> noteIds)
        {
            if (noteIds == null || !noteIds.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();
                var results = new List<object>();
                var successCount = 0;
                var failureCount = 0;

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        // Verify note exists and is in trash
                        var noteQuery = new GetNoteByIdQuery
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        var currentNote = await _mediator.Send(noteQuery);
                        if (currentNote == null)
                        {
                            results.Add(new { noteId, status = "failed", reason = "Note not found" });
                            failureCount++;
                            continue;
                        }

                        if (!currentNote.IsTrashed)
                        {
                            results.Add(new { noteId, status = "failed", reason = "Note must be in trash" });
                            failureCount++;
                            continue;
                        }

                        // Permanently delete
                        var command = new DeleteNoteCommand
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        await _mediator.Send(command);
                        results.Add(new { noteId, status = "success" });
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to permanently delete note {NoteId}", noteId);
                        results.Add(new { noteId, status = "failed", reason = "Internal error" });
                        failureCount++;
                    }
                }

                _logger.LogInformation("Bulk permanent delete completed: {SuccessCount} success, {FailureCount} failures", successCount, failureCount);

                return Ok(new
                {
                    message = $"Bulk permanent delete completed: {successCount} success, {failureCount} failures",
                    successCount,
                    failureCount,
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk permanently deleting notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to permanently delete notes. Please try again.");
            }
        }

        /// <summary>
        /// Bulk delete notes (legacy endpoint - kept for backward compatibility)
        /// </summary>
        /// <param name="noteIds">List of note IDs to delete</param>
        /// <returns>Success status</returns>
        [HttpDelete("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> BulkDeleteNotes([FromBody] List<int> noteIds)
        {
            if (noteIds == null || !noteIds.Any())
            {
                return BadRequest("Note IDs list cannot be empty");
            }

            try
            {
                var userId = GetCurrentUserId();

                foreach (var noteId in noteIds)
                {
                    var command = new DeleteNoteCommand
                    {
                        Id = noteId,
                        UserId = userId
                    };

                    await _mediator.Send(command);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to delete notes. Please try again.");
            }
        }

        /// <summary>
        /// Test endpoint to check authentication and user context
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("test-auth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult TestAuth()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

                return Ok(new
                {
                    userId = userId,
                    isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                    authType = User.Identity?.AuthenticationType,
                    claims = userClaims,
                    hasCurrentUser = _currentUserService.HasCurrentUser(),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test-auth endpoint");
                return Unauthorized(new { error = ex.Message, timestamp = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Pin/Unpin single note (matches frontend request format)
        /// </summary>
        /// <param name="request">Pin/Unpin request with single note ID</param>
        /// <returns>Updated note</returns>
        [HttpPost("pinUnpinNotes")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<NoteDto>> PinUnpinNotes([FromBody] PinUnpinNoteRequest request)
        {
            // Debug logging
            _logger.LogInformation("Received pin/unpin request: NoteId={NoteId}, IsPinned={IsPinned}",
                request?.NoteId, request?.IsPinned);

            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (request.NoteId <= 0)
            {
                return BadRequest("Valid note ID must be provided");
            }

            try
            {
                var userId = GetCurrentUserId();

                _logger.LogInformation("Processing pin/unpin request for note {NoteId} by user {UserId}. Setting IsPinned to {IsPinned}",
                    request.NoteId, userId, request.IsPinned);

                // Use the CQRS command for single note pin operation
                var command = new PinUnpinNoteCommand
                {
                    NoteId = request.NoteId,
                    IsPinned = request.IsPinned,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                _logger.LogInformation("Successfully updated pin status for note {NoteId}", request.NoteId);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {request.NoteId} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authentication error during pin/unpin operation");
                return Unauthorized("You don't have permission to modify this note");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinning/unpinning note {NoteId} for user: {UserId}", request.NoteId, GetCurrentUserId());
                return BadRequest("Failed to pin/unpin note. Please try again.");
            }
        }

        /// <summary>
        /// Pin/Unpin multiple notes (bulk operation)
        /// </summary>
        /// <param name="request">Bulk Pin/Unpin request</param>
        /// <returns>Success status</returns>
        [HttpPost("bulkPinUnpinNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> BulkPinUnpinNotes([FromBody] BulkPinDto request)
        {
            // Debug logging
            _logger.LogInformation("Received bulk pin/unpin request: NoteIdList={NoteIdList}, IsPinned={IsPinned}",
                string.Join(",", request?.NoteIdList ?? new List<string>()), request?.IsPinned);

            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (!request.HasValidNoteIds())
            {
                return BadRequest("At least one valid note ID must be provided");
            }

            try
            {
                var userId = GetCurrentUserId();
                var noteIds = request.GetNoteIds();

                _logger.LogInformation("Processing bulk pin/unpin request for {Count} notes by user {UserId}. Note IDs: [{NoteIds}]",
                    noteIds.Count, userId, string.Join(",", noteIds));

                // Use the CQRS command for bulk pin operation
                var command = new BulkPinNotesCommand
                {
                    NoteIds = noteIds,
                    IsPinned = request.IsPinned,
                    UserId = userId
                };

                var result = await _mediator.Send(command);

                var response = new
                {
                    message = result.Message,
                    successCount = result.SuccessCount,
                    failedCount = result.FailedCount,
                    updatedNotes = result.UpdatedNotes,
                    failedNoteIds = result.FailedCount > 0 ? result.FailedNoteIds : null
                };

                // Return BadRequest if all operations failed, otherwise return Ok
                if (result.FailedCount > 0 && result.SuccessCount == 0)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Authentication error during bulk pin/unpin operation");
                return Unauthorized("Authentication required. Please log in again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk pinning/unpinning notes for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to pin/unpin notes. Please try again.");
            }
        }

        /// <summary>
        /// Add or update reminder for notes
        /// </summary>
        /// <param name="request">Reminder request</param>
        /// <returns>Success status</returns>
        [HttpPost("addUpdateReminderNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> AddUpdateReminderNotes([FromBody] BulkReminderDto request)
        {
            // Debug logging
            _logger.LogInformation("Received reminder request: NoteIds={NoteIds}, ReminderDateTime={ReminderDateTime}, CurrentUtc={CurrentUtc}",
                string.Join(",", request.NoteIdList ?? new List<string>()),
                request.ReminderDateTime?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

            // Enhanced validation
            //if (!request.IsValid(out var validationErrors))
            //{
            //    _logger.LogWarning("Reminder validation failed: {Errors}", string.Join("; ", validationErrors));
            //    return BadRequest(new
            //    {
            //        message = "Validation failed",
            //        errors = validationErrors,
            //        receivedData = new
            //        {
            //            noteIdList = request.NoteIdList,
            //            reminderDateTime = request.ReminderDateTime?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            //            currentUtcTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            //        },
            //        expectedFormat = new
            //        {
            //            noteIdList = new[] { "1", "2", "3" },
            //            reminderDateTime = "2024-12-31T10:00:00Z"
            //        }
            //    });
            //}

            try
            {
                var userId = GetCurrentUserId();
                var updatedNotes = new List<NoteDto>();
                var noteIds = request.GetNoteIds();

                _logger.LogInformation("Adding/updating reminders for {Count} notes by user {UserId}", noteIds.Count, userId);

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        // Get the current note
                        var noteQuery = new GetNoteByIdQuery
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        var currentNote = await _mediator.Send(noteQuery);
                        if (currentNote != null)
                        {
                            // Update with reminder
                            var command = new UpdateNoteCommand
                            {
                                Id = noteId,
                                Title = currentNote.Title,
                                Content = currentNote.Content,
                                Color = currentNote.Color,
                                ReminderDateTime = request.ReminderDateTime,
                                LabelIds = currentNote.Labels?.Where(l => l != null).Select(l => l.Id).ToList() ?? new List<int>(),
                                UserId = userId
                            };

                            var result = await _mediator.Send(command);
                            updatedNotes.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add/update reminder for note {NoteId}", noteId);
                    }
                }

                return Ok(new { message = $"Reminder updated for {updatedNotes.Count} notes", updatedNotes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding/updating reminders for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to add/update reminders. Please try again.");
            }
        }

        /// <summary>
        /// Remove reminder from notes
        /// </summary>
        /// <param name="request">Remove reminder request</param>
        /// <returns>Success status</returns>
        [HttpPost("removeReminderNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RemoveReminderNotes([FromBody] BulkRemoveReminderDto request)
        {
            // Enhanced validation
            if (!request.IsValid(out var validationErrors))
            {
                return BadRequest(new
                {
                    message = "Validation failed",
                    errors = validationErrors,
                    expectedFormat = new
                    {
                        noteIdList = new[] { "1", "2", "3" }
                    }
                });
            }

            try
            {
                var userId = GetCurrentUserId();
                var updatedNotes = new List<NoteDto>();
                var noteIds = request.GetNoteIds();

                _logger.LogInformation("Removing reminders for {Count} notes by user {UserId}", noteIds.Count, userId);

                foreach (var noteId in noteIds)
                {
                    try
                    {
                        // Get the current note
                        var noteQuery = new GetNoteByIdQuery
                        {
                            Id = noteId,
                            UserId = userId
                        };

                        var currentNote = await _mediator.Send(noteQuery);
                        if (currentNote != null)
                        {
                            // Update to remove reminder
                            var command = new UpdateNoteCommand
                            {
                                Id = noteId,
                                Title = currentNote.Title,
                                Content = currentNote.Content,
                                Color = currentNote.Color,
                                ReminderDateTime = null, // Remove reminder
                                LabelIds = currentNote.Labels?.Where(l => l != null).Select(l => l.Id).ToList() ?? new List<int>(),
                                UserId = userId
                            };

                            var result = await _mediator.Send(command);
                            updatedNotes.Add(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to remove reminder for note {NoteId}", noteId);
                    }
                }

                return Ok(new { message = $"Reminder removed for {updatedNotes.Count} notes", updatedNotes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing reminders for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to remove reminders. Please try again.");
            }
        }

        /// <summary>
        /// Create note from template
        /// </summary>
        /// <param name="templateId">Template ID</param>
        /// <returns>Created note</returns>
        [HttpPost("from-template/{templateId}")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> CreateNoteFromTemplate(int templateId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new CreateNoteFromTemplateCommand
                {
                    TemplateId = templateId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetNoteById), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Template with ID {templateId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note from template {TemplateId} for user: {UserId}", templateId, GetCurrentUserId());
                return BadRequest("Failed to create note from template. Please try again.");
            }
        }

        /// <summary>
        /// Get note history
        /// </summary>
        /// <param name="id">Note ID</param>
        /// <returns>Note history</returns>
        [HttpGet("{id}/history")]
        [ProducesResponseType(typeof(List<NoteHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteHistoryDto>>> GetNoteHistory(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetNoteHistoryQuery
                {
                    NoteId = id,
                    UserId = userId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Note with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history for note {NoteId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to get note history. Please try again.");
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        /// <returns>User ID</returns>
        private async Task<int> GetCurrentUserIdAsync()
        {
            try
            {
                return await _currentUserService.GetCurrentUserIdAsync();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error getting current user ID from service, using fallback");

                // Fallback: Try to get user ID directly from JWT claims
                var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    _logger.LogInformation("Retrieved user ID {UserId} from JWT claims", userId);
                    return userId;
                }

                // If no JWT token, check if we're in development mode and use fallback
                var environment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (environment.IsDevelopment())
                {
                    _logger.LogWarning("Using fallback user ID 2 for testing");
                    return 2; // Fallback user ID for development/testing
                }

                throw new UnauthorizedAccessException("Authentication required. Please log in.");
            }
        }

        /// <summary>
        /// Add a label to a note
        /// </summary>
        /// <param name="request">Add label request</param>
        /// <returns>Success status</returns>
        [HttpPost("labels/add")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> AddLabelToNote([FromBody] AddLabelToNoteDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new AddLabelToNoteCommand
                {
                    NoteId = request.NoteId,
                    LabelId = request.LabelId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding label {LabelId} to note {NoteId} for user: {UserId}",
                    request.LabelId, request.NoteId, GetCurrentUserId());
                return BadRequest("Failed to add label to note. Please try again.");
            }
        }

        /// <summary>
        /// Remove a label from a note
        /// </summary>
        /// <param name="request">Remove label request</param>
        /// <returns>Success status</returns>
        [HttpPost("labels/remove")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<bool>> RemoveLabelFromNote([FromBody] RemoveLabelFromNoteDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new RemoveLabelFromNoteCommand
                {
                    NoteId = request.NoteId,
                    LabelId = request.LabelId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing label {LabelId} from note {NoteId} for user: {UserId}",
                    request.LabelId, request.NoteId, GetCurrentUserId());
                return BadRequest("Failed to remove label from note. Please try again.");
            }
        }

        /// <summary>
        /// Add a label to multiple notes (bulk operation)
        /// </summary>
        /// <param name="request">Bulk add label request</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("labels/bulk-add")]
        [ProducesResponseType(typeof(BulkAddLabelResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BulkAddLabelResult>> BulkAddLabel([FromBody] BulkAddLabelDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new BulkAddLabelCommand
                {
                    NoteIds = request.NoteIds,
                    LabelId = request.LabelId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk adding label {LabelId} to notes for user: {UserId}",
                    request.LabelId, GetCurrentUserId());
                return BadRequest("Failed to add label to notes. Please try again.");
            }
        }

        /// <summary>
        /// Remove a label from multiple notes (bulk operation)
        /// </summary>
        /// <param name="request">Bulk remove label request</param>
        /// <returns>Bulk operation result</returns>
        [HttpPost("labels/bulk-remove")]
        [ProducesResponseType(typeof(BulkRemoveLabelResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<BulkRemoveLabelResult>> BulkRemoveLabel([FromBody] BulkRemoveLabelDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new BulkRemoveLabelCommand
                {
                    NoteIds = request.NoteIds,
                    LabelId = request.LabelId,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk removing label {LabelId} from notes for user: {UserId}",
                    request.LabelId, GetCurrentUserId());
                return BadRequest("Failed to remove label from notes. Please try again.");
            }
        }

        /// <summary>
        /// Manage all labels for a note (replace existing labels with new set)
        /// </summary>
        /// <param name="request">Manage note labels request</param>
        /// <returns>Updated note with new labels</returns>
        [HttpPut("labels/manage")]
        [ProducesResponseType(typeof(NoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteDto>> ManageNoteLabels([FromBody] ManageNoteLabelsDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new ManageNoteLabelsCommand
                {
                    NoteId = request.NoteId,
                    LabelIds = request.LabelIds,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing labels for note {NoteId} for user: {UserId}",
                    request.NoteId, GetCurrentUserId());
                return BadRequest("Failed to manage note labels. Please try again.");
            }
        }

        /// <summary>
        /// Synchronous wrapper for GetCurrentUserIdAsync to avoid changing all method signatures
        /// </summary>
        /// <returns>User ID</returns>
        private int GetCurrentUserId()
        {
            return GetCurrentUserIdAsync().GetAwaiter().GetResult();
        }


    }
}
