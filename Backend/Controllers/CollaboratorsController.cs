using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Collaborators.Commands.AddCollaborator;
using fundoo_notes.Application.Features.Collaborators.Commands.RemoveCollaborator;
using fundoo_notes.Application.Features.Collaborators.Commands.UpdateCollaboratorPermission;
using fundoo_notes.Application.Features.Collaborators.Queries.GetNoteCollaborators;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Collaborators controller for managing note sharing
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class CollaboratorsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CollaboratorsController> _logger;

        public CollaboratorsController(IMediator mediator, ILogger<CollaboratorsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get collaborators for a specific note
        /// </summary>
        /// <param name="noteId">Note ID</param>
        /// <returns>List of collaborators</returns>
        [HttpGet("note/{noteId}")]
        [ProducesResponseType(typeof(List<CollaboratorDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<CollaboratorDto>>> GetNoteCollaborators(int noteId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetNoteCollaboratorsQuery
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
                _logger.LogError(ex, "Error getting collaborators for note {NoteId} by user: {UserId}", noteId, GetCurrentUserId());
                return BadRequest("Failed to get collaborators. Please try again.");
            }
        }

        /// <summary>
        /// Add a collaborator to a note
        /// </summary>
        /// <param name="request">Collaborator data</param>
        /// <returns>Created collaborator</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CollaboratorDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CollaboratorDto>> AddCollaborator([FromBody] AddCollaboratorDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new AddCollaboratorCommand
                {
                    NoteId = request.NoteId,
                    UserEmail = request.UserEmail,
                    Permission = request.Permission,
                    RequesterId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetNoteCollaborators), new { noteId = request.NoteId }, result);
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
                _logger.LogError(ex, "Error adding collaborator for note {NoteId} by user: {UserId}", request.NoteId, GetCurrentUserId());
                return BadRequest("Failed to add collaborator. Please try again.");
            }
        }

        /// <summary>
        /// Remove a collaborator from a note
        /// </summary>
        /// <param name="id">Collaborator ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> RemoveCollaborator(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new RemoveCollaboratorCommand
                {
                    Id = id,
                    RequesterId = userId
                };

                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Collaborator with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing collaborator {CollaboratorId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to remove collaborator. Please try again.");
            }
        }

        /// <summary>
        /// Update collaborator permission
        /// </summary>
        /// <param name="id">Collaborator ID</param>
        /// <param name="request">Permission update data</param>
        /// <returns>Updated collaborator</returns>
        [HttpPatch("{id}/permission")]
        [ProducesResponseType(typeof(CollaboratorDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<CollaboratorDto>> UpdateCollaboratorPermission(int id, [FromBody] UpdateCollaboratorPermissionDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new UpdateCollaboratorPermissionCommand
                {
                    Id = id,
                    Permission = request.Permission,
                    RequesterId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Collaborator with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating collaborator {CollaboratorId} permission by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to update collaborator permission. Please try again.");
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
