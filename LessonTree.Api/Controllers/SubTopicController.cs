using LessonTree.BLL.Service;
using LessonTree.DAL.Domain;
using LessonTree.Models.DTO;
using LessonTree.Models.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LessonTree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SubTopicController : ControllerBase
    {
        private readonly ISubTopicService _service;
        private readonly ILogger<SubTopicController> _logger;

        public SubTopicController(ISubTopicService service, ILogger<SubTopicController> logger)
        {
            _service = service;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Failed to extract UserId from JWT claims");
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubTopic(int id)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching subtopic ID: {SubTopicId} for User ID: {UserId}", id, userId);

            try
            {
                var subTopic = await _service.GetByIdAsync(id, userId);
                return Ok(subTopic);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found or not owned by User ID {UserId}", id, userId);
                return NotFound(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubTopics(ArchiveFilter filter = ArchiveFilter.Active)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching all subtopics for User ID: {UserId}, Filter: {Filter}", userId, filter);
            var subTopics = await _service.GetAllAsync(userId, filter);
            return Ok(subTopics);
        }

        [HttpGet("byTopic/{topicId}")]
        public async Task<IActionResult> GetSubtopicsByTopicId(int topicId)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Fetching subtopics for Topic ID: {TopicId}, User ID: {UserId}", topicId, userId);
            var subTopics = await _service.GetSubtopicsByTopicIdAsync(topicId, userId);
            return Ok(subTopics);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubTopic([FromBody] SubTopicCreateResource subTopicCreateResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Adding subtopic with Title: {Title} for User ID: {UserId}", subTopicCreateResource.Title, userId);

            var createdId = await _service.AddAsync(subTopicCreateResource, userId);
            var createdSubTopic = await _service.GetByIdAsync(createdId, userId);
            _logger.LogInformation("Added subtopic with ID: {SubTopicId}, Title: {Title}", createdSubTopic.Id, createdSubTopic.Title);
            return CreatedAtAction(nameof(GetSubTopic), new { id = createdSubTopic.Id }, createdSubTopic);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubTopic(int id, [FromBody] SubTopicUpdateResource subTopicUpdateResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Updating subtopic ID: {SubTopicId} for User ID: {UserId}", id, userId);

            if (id != subTopicUpdateResource.Id)
            {
                _logger.LogWarning("ID mismatch: URL ID {UrlId} does not match body ID {BodyId}", id, subTopicUpdateResource.Id);
                return BadRequest();
            }

            var existingSubTopic = await _service.GetDomainSubTopicByIdAsync(id);
            if (existingSubTopic == null)
            {
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found", id);
                return NotFound($"SubTopic with ID {id} not found.");
            }
            if (existingSubTopic.UserId != userId)
            {
                _logger.LogWarning("User ID {UserId} attempted to update subtopic ID {SubTopicId} owned by another user", userId, id);
                return Forbid();
            }

            await _service.UpdateAsync(subTopicUpdateResource);
            _logger.LogInformation("Updated subtopic with ID: {SubTopicId}, Title: {Title}", id, subTopicUpdateResource.Title);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTopic(int id)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Deleting subtopic ID: {SubTopicId} for User ID: {UserId}", id, userId);

            var subTopic = await _service.GetDomainSubTopicByIdAsync(id);
            if (subTopic == null)
            {
                _logger.LogWarning("SubTopic with ID {SubTopicId} not found", id);
                return NotFound($"SubTopic with ID {id} not found.");
            }
            if (subTopic.UserId != userId)
            {
                _logger.LogWarning("User ID {UserId} attempted to delete subtopic ID {SubTopicId} owned by another user", userId, id);
                return Forbid();
            }

            try
            {
                await _service.DeleteAsync(id);
                _logger.LogInformation("Deleted subtopic with ID: {SubTopicId}", id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete subtopic: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveSubTopic([FromBody] SubTopicMoveResource moveResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Moving SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId} for User ID: {UserId}",
                moveResource.SubTopicId, moveResource.NewTopicId, userId);

            var subTopic = await _service.GetDomainSubTopicByIdAsync(moveResource.SubTopicId);
            if (subTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", moveResource.SubTopicId);
                return NotFound($"SubTopic with ID {moveResource.SubTopicId} not found.");
            }
            if (subTopic.UserId != userId)
            {
                _logger.LogWarning("User ID {UserId} attempted to move subtopic ID {SubTopicId} owned by another user", userId, moveResource.SubTopicId);
                return Forbid();
            }

            try
            {
                await _service.MoveSubTopic(moveResource.SubTopicId, moveResource.NewTopicId);
                _logger.LogInformation("Moved SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId}",
                    moveResource.SubTopicId, moveResource.NewTopicId);
                return Ok(new { status = "success", message = "SubTopic moved successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error moving subtopic: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error moving SubTopic ID: {SubTopicId}", moveResource.SubTopicId);
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("copy")]
        public async Task<IActionResult> CopySubTopic([FromBody] SubTopicMoveResource copyResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Copying SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId} for User ID: {UserId}",
                copyResource.SubTopicId, copyResource.NewTopicId, userId);

            var subTopic = await _service.GetDomainSubTopicByIdAsync(copyResource.SubTopicId);
            if (subTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", copyResource.SubTopicId);
                return NotFound($"SubTopic with ID {copyResource.SubTopicId} not found.");
            }

            try
            {
                var newSubTopic = await _service.CopySubTopicAsync(copyResource.SubTopicId, copyResource.NewTopicId, userId);
                _logger.LogInformation("Copied SubTopic ID: {SubTopicId} to new SubTopic ID: {NewSubTopicId}",
                    copyResource.SubTopicId, newSubTopic.Id);
                return CreatedAtAction(nameof(GetSubTopic), new { id = newSubTopic.Id }, newSubTopic);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Error copying subtopic: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error copying SubTopic ID: {SubTopicId}", copyResource.SubTopicId);
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // Add SortOrder endpoint
        [HttpPut("{subTopicId}/sortOrder")]
        public async Task<IActionResult> UpdateSubTopicSortOrder(int subTopicId, [FromBody] int sortOrder)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Updating sort order for SubTopic ID: {SubTopicId} to {SortOrder} for User ID: {UserId}", subTopicId, sortOrder, userId);

            var subTopic = await _service.GetDomainSubTopicByIdAsync(subTopicId);
            if (subTopic == null)
            {
                _logger.LogError("SubTopic with ID {SubTopicId} not found", subTopicId);
                return NotFound();
            }
            if (subTopic.UserId != userId)
            {
                _logger.LogWarning("User ID {UserId} attempted to update sort order for subtopic ID {SubTopicId} owned by another user", userId, subTopicId);
                return Forbid();
            }

            try
            {
                await _service.UpdateSortOrderAsync(subTopicId, sortOrder);
                _logger.LogInformation("Updated sort order for SubTopic ID: {SubTopicId} to {SortOrder}", subTopicId, sortOrder);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sort order for SubTopic ID: {SubTopicId}", subTopicId);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}