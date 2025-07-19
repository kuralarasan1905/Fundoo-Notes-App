using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Templates.Commands.CreateTemplate;
using fundoo_notes.Application.Features.Templates.Commands.UpdateTemplate;
using fundoo_notes.Application.Features.Templates.Commands.DeleteTemplate;
using fundoo_notes.Application.Features.Templates.Queries.GetUserTemplates;
using fundoo_notes.Application.Features.Templates.Queries.GetPublicTemplates;
using fundoo_notes.Application.Features.Templates.Queries.GetTemplateById;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Templates controller for managing note templates
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class TemplatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TemplatesController> _logger;

        public TemplatesController(IMediator mediator, ILogger<TemplatesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get user's templates
        /// </summary>
        /// <param name="category">Filter by category</param>
        /// <returns>List of user templates</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<NoteTemplateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<NoteTemplateDto>>> GetUserTemplates([FromQuery] string? category = null)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetUserTemplatesQuery
                {
                    UserId = userId,
                    Category = category
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting templates for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to get templates. Please try again.");
            }
        }

        /// <summary>
        /// Get public templates
        /// </summary>
        /// <param name="category">Filter by category</param>
        /// <returns>List of public templates</returns>
        [HttpGet("public")]
        [ProducesResponseType(typeof(List<NoteTemplateDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<NoteTemplateDto>>> GetPublicTemplates([FromQuery] string? category = null)
        {
            try
            {
                var query = new GetPublicTemplatesQuery
                {
                    Category = category
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public templates");
                return BadRequest("Failed to get public templates. Please try again.");
            }
        }

        /// <summary>
        /// Get template by ID
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Template details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(NoteTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteTemplateDto>> GetTemplateById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var query = new GetTemplateByIdQuery
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound($"Template with ID {id} not found");
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting template {TemplateId} for user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to retrieve template. Please try again.");
            }
        }

        /// <summary>
        /// Create a new template
        /// </summary>
        /// <param name="request">Template creation data</param>
        /// <returns>Created template</returns>
        [HttpPost]
        [ProducesResponseType(typeof(NoteTemplateDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteTemplateDto>> CreateTemplate([FromBody] CreateNoteTemplateDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new CreateTemplateCommand
                {
                    Name = request.Name,
                    Description = request.Description,
                    Title = request.Title,
                    Content = request.Content,
                    Color = request.Color,
                    Category = request.Category,
                    IsPublic = request.IsPublic,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating template for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to create template. Please try again.");
            }
        }

        /// <summary>
        /// Update an existing template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <param name="request">Template update data</param>
        /// <returns>Updated template</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(NoteTemplateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<NoteTemplateDto>> UpdateTemplate(int id, [FromBody] CreateNoteTemplateDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new UpdateTemplateCommand
                {
                    Id = id,
                    Name = request.Name,
                    Description = request.Description,
                    Title = request.Title,
                    Content = request.Content,
                    Color = request.Color,
                    Category = request.Category,
                    IsPublic = request.IsPublic,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Template with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating template {TemplateId} for user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to update template. Please try again.");
            }
        }

        /// <summary>
        /// Delete a template
        /// </summary>
        /// <param name="id">Template ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteTemplate(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new DeleteTemplateCommand
                {
                    Id = id,
                    UserId = userId
                };

                await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Template with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting template {TemplateId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to delete template. Please try again.");
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
