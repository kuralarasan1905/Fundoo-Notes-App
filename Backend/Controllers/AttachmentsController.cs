using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Attachments.Commands.UploadAttachment;
using fundoo_notes.Application.Features.Attachments.Commands.DeleteAttachment;
using fundoo_notes.Application.Features.Attachments.Queries.GetAttachments;
using fundoo_notes.Application.Features.Attachments.Queries.GetAttachmentForDownload;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Attachments controller for managing note file attachments
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(IMediator mediator, ILogger<AttachmentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get attachments for a specific note
        /// </summary>
        /// <param name="noteId">Note ID</param>
        /// <returns>List of attachments</returns>
        [HttpGet("note/{noteId}")]
        [ProducesResponseType(typeof(List<NoteAttachmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteAttachmentDto>>> GetNoteAttachments(int noteId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetAttachmentsQuery
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
                _logger.LogError(ex, "Error getting attachments for note {NoteId} by user: {UserId}", noteId, GetCurrentUserId());
                return BadRequest("Failed to get attachments. Please try again.");
            }
        }

        /// <summary>
        /// Upload an attachment to a note
        /// </summary>
        /// <param name="noteId">Note ID</param>
        /// <param name="file">File to upload</param>
        /// <returns>Created attachment</returns>
        [HttpPost("note/{noteId}")]
        [ProducesResponseType(typeof(NoteAttachmentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10MB limit
        public async Task<ActionResult<NoteAttachmentDto>> UploadAttachment(int noteId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file provided");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf", "text/plain" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest("File type not supported");
                }

                // Validate file size (10MB max)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest("File size exceeds 10MB limit");
                }

                var userId = GetCurrentUserId();

                var command = new UploadAttachmentCommand
                {
                    NoteId = noteId,
                    File = file,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetNoteAttachments), new { noteId = noteId }, result);
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
                _logger.LogError(ex, "Error uploading attachment for note {NoteId} by user: {UserId}", noteId, GetCurrentUserId());
                return BadRequest("Failed to upload attachment. Please try again.");
            }
        }

        /// <summary>
        /// Delete an attachment
        /// </summary>
        /// <param name="id">Attachment ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteAttachment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new DeleteAttachmentCommand
                {
                    Id = id,
                    UserId = userId
                };

                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Attachment with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment {AttachmentId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to delete attachment. Please try again.");
            }
        }

        /// <summary>
        /// Download an attachment
        /// </summary>
        /// <param name="id">Attachment ID</param>
        /// <returns>File download</returns>
        [HttpGet("{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DownloadAttachment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetAttachmentForDownloadQuery
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound($"Attachment with ID {id} not found");
                }

                // Return file from storage
                var fileBytes = await System.IO.File.ReadAllBytesAsync(result.FilePath);
                return File(fileBytes, result.ContentType, result.FileName);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Attachment with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment {AttachmentId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to download attachment. Please try again.");
            }
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        /// <returns>User ID</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return userId;
        }
    }
}
