// RESPONSIBILITY: Handles HTTP requests for SubTopic CRUD operations
// DOES NOT: Handle business logic or data access directly
// CALLED BY: Angular UI via HTTP requests

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
    public class SubTopicController : BaseController
    {
        private readonly ISubTopicService _service;
        private readonly ILogger<SubTopicController> _logger;

        public SubTopicController(ISubTopicService service, ILogger<SubTopicController> logger)
        {
            _service = service;
            _logger = logger;
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

            try
            {
                var updatedSubTopic = await _service.UpdateAsync(subTopicUpdateResource, userId); // Service handles ownership validation
                _logger.LogInformation("Updated subtopic with ID: {SubTopicId}, Title: {Title} by User ID: {UserId}", id, subTopicUpdateResource.Title, userId);
                return Ok(updatedSubTopic);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("SubTopic update failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized subtopic update attempt for ID: {SubTopicId} by User ID: {UserId}", id, userId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("SubTopic update invalid operation: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTopic(int id)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Deleting subtopic ID: {SubTopicId} for User ID: {UserId}", id, userId);

            try
            {
                await _service.DeleteAsync(id, userId); // Service handles ownership validation
                _logger.LogInformation("Deleted subtopic with ID: {SubTopicId} by User ID: {UserId}", id, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("SubTopic deletion failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized subtopic deletion attempt for ID: {SubTopicId} by User ID: {UserId}", id, userId);
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete subtopic: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("copy")]
        public async Task<IActionResult> CopySubTopic([FromBody] SubTopicMoveResource copyResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Copying SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId} for User ID: {UserId}",
                copyResource.SubTopicId, copyResource.NewTopicId, userId);

            try
            {
                var newSubTopic = await _service.CopySubTopicAsync(copyResource.SubTopicId, copyResource.NewTopicId, userId);
                _logger.LogInformation("Copied SubTopic ID: {SubTopicId} to new SubTopic ID: {NewSubTopicId} by User ID: {UserId}",
                    copyResource.SubTopicId, newSubTopic.Id, userId);
                return CreatedAtAction(nameof(GetSubTopic), new { id = newSubTopic.Id }, newSubTopic);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("SubTopic copy failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error copying SubTopic ID: {SubTopicId}", copyResource.SubTopicId);
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("move")]
        public async Task<IActionResult> MoveSubTopic([FromBody] SubTopicMoveResource moveResource)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Moving SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId} for User ID: {UserId}",
                moveResource.SubTopicId, moveResource.NewTopicId, userId);

            try
            {
                var movedSubTopic = await _service.MoveSubTopicAsync(moveResource, userId);
                _logger.LogInformation("Moved SubTopic ID: {SubTopicId} to Topic ID: {NewTopicId} by User ID: {UserId}",
                    moveResource.SubTopicId, moveResource.NewTopicId, userId);
                
                var result = new SubTopicPositioningResult
                {
                    IsSuccess = true,
                    SubTopicId = movedSubTopic.Id,
                    NewTopicId = movedSubTopic.TopicId,
                    TargetSortOrder = movedSubTopic.SortOrder,
                    ModifiedEntities = new List<ModifiedEntityInfo>
                    {
                        new ModifiedEntityInfo
                        {
                            EntityId = movedSubTopic.Id,
                            EntityType = "SubTopic",
                            NewSortOrder = movedSubTopic.SortOrder,
                            ParentId = movedSubTopic.TopicId,
                            ParentType = "Topic"
                        }
                    }
                };
                
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("SubTopic move failed: {Message}", ex.Message);
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized SubTopic move attempt for ID: {SubTopicId} by User ID: {UserId}", moveResource.SubTopicId, userId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error moving SubTopic ID: {SubTopicId}", moveResource.SubTopicId);
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        // Add SortOrder endpoint
        [HttpPut("{subTopicId}/sortOrder")]
        public async Task<IActionResult> UpdateSubTopicSortOrder(int subTopicId, [FromBody] int sortOrder)
        {
            int userId = GetCurrentUserId();
            _logger.LogDebug("Updating sort order for SubTopic ID: {SubTopicId} to {SortOrder} for User ID: {UserId}", subTopicId, sortOrder, userId);

            try
            {
                await _service.UpdateSortOrderAsync(subTopicId, sortOrder, userId); // Service handles ownership validation
                _logger.LogInformation("Updated sort order for SubTopic ID: {SubTopicId} to {SortOrder} by User ID: {UserId}", subTopicId, sortOrder, userId);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Sort order update failed: {Message}", ex.Message);
                return NotFound(new { status = "error", message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Unauthorized sort order update attempt for SubTopic ID: {SubTopicId} by User ID: {UserId}", subTopicId, userId);
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sort order for SubTopic ID: {SubTopicId}", subTopicId);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}