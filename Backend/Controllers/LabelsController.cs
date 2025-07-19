using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using fundoo_notes.Application.DTOs;
using fundoo_notes.Application.Features.Labels.Commands.CreateLabel;
using fundoo_notes.Application.Features.Labels.Commands.UpdateLabel;
using fundoo_notes.Application.Features.Labels.Commands.DeleteLabel;
using fundoo_notes.Application.Features.Labels.Queries.GetUserLabels;

namespace fundoo_notes.Controllers
{
    /// <summary>
    /// Labels controller for managing user labels
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class LabelsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LabelsController> _logger;

        public LabelsController(IMediator mediator, ILogger<LabelsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's labels
        /// </summary>
        /// <returns>List of user labels</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<LabelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<LabelDto>>> GetLabels()
        {
            var userId = GetCurrentUserId();
            
            var query = new GetUserLabelsQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Create a new label
        /// </summary>
        /// <param name="request">Label creation data</param>
        /// <returns>Created label</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LabelDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LabelDto>> CreateLabel([FromBody] CreateLabelDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new CreateLabelCommand
                {
                    Name = request.Name,
                    Color = request.Color,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetLabels), new { }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Label creation failed for user: {UserId}", GetCurrentUserId());
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating label for user: {UserId}", GetCurrentUserId());
                return BadRequest("Failed to create label. Please try again.");
            }
        }

        /// <summary>
        /// Update an existing label
        /// </summary>
        /// <param name="id">Label ID</param>
        /// <param name="request">Label update data</param>
        /// <returns>Updated label</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(LabelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LabelDto>> UpdateLabel(int id, [FromBody] UpdateLabelDto request)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new UpdateLabelCommand
                {
                    Id = id,
                    Name = request.Name,
                    Color = request.Color,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Label with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Label update failed for user: {UserId}", GetCurrentUserId());
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating label {LabelId} for user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to update label. Please try again.");
            }
        }

        /// <summary>
        /// Delete a label
        /// </summary>
        /// <param name="id">Label ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteLabel(int id)
        {
            try
            {
                var userId = GetCurrentUserId();

                var command = new DeleteLabelCommand
                {
                    Id = id,
                    UserId = userId
                };

                var result = await _mediator.Send(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Label with ID {id} not found");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting label {LabelId} by user: {UserId}", id, GetCurrentUserId());
                return BadRequest("Failed to delete label. Please try again.");
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
