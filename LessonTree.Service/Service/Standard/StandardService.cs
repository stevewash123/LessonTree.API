using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.BLL.Service
{
    public class StandardService : IStandardService
    {
        private readonly IStandardRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<StandardService> _logger;

        public StandardService(IStandardRepository repository, IMapper mapper, ILogger<StandardService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        // PARTIAL FILE - Integration: Replace the entry point logging in key business methods
        // Focus: Standardize entry point logging and reduce excessive debug logging

        public async Task<List<StandardResource>> GetAllAsync()
        {
            _logger.LogInformation("GetAllAsync: Fetching all standards");
            try
            {
                var standards = await _repository.GetAll().ToListAsync();
                _logger.LogInformation("GetAllAsync: Fetched {Count} standards", standards.Count);
                return _mapper.Map<List<StandardResource>>(standards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch all standards");
                throw;
            }
        }

        public async Task<StandardResource?> GetByIdAsync(int id)
        {
            _logger.LogInformation("GetByIdAsync: Fetching standard by ID: {StandardId}", id);
            try
            {
                var standard = await _repository.GetByIdAsync(id);
                if (standard == null)
                {
                    _logger.LogWarning("Standard with ID {StandardId} not found", id);
                    return null;
                }
                _logger.LogInformation("GetByIdAsync: Standard with ID {StandardId} found", standard.Id);
                return _mapper.Map<StandardResource>(standard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch standard with ID: {StandardId}", id);
                throw;
            }
        }

        public async Task<int> AddAsync(StandardCreateResource standardCreateResource)
        {
            _logger.LogInformation("AddAsync: Adding standard: {Title}", standardCreateResource.Title);
            try
            {
                var standard = _mapper.Map<Standard>(standardCreateResource);
                var createdId = await _repository.AddAsync(standard);
                _logger.LogInformation("AddAsync: Standard added with ID: {StandardId}", createdId);
                return createdId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add standard: {Title}", standardCreateResource.Title);
                throw;
            }
        }

        public async Task<StandardResource> UpdateAsync(StandardUpdateResource standardUpdateResource)
        {
            _logger.LogInformation("UpdateAsync: Updating standard with ID: {StandardId}", standardUpdateResource.Id);
            try
            {
                var existingStandard = await _repository.GetByIdAsync(standardUpdateResource.Id);
                if (existingStandard == null)
                {
                    _logger.LogWarning("Standard with ID {StandardId} not found for update", standardUpdateResource.Id);
                    throw new ArgumentException("Standard not found");
                }
                _mapper.Map(standardUpdateResource, existingStandard);
                await _repository.UpdateAsync(existingStandard);
                _logger.LogInformation("UpdateAsync: Standard updated with ID: {StandardId}", existingStandard.Id);

                // Return the updated entity
                return await GetByIdAsync(existingStandard.Id) ?? throw new InvalidOperationException("Updated standard could not be retrieved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update standard with ID: {StandardId}", standardUpdateResource.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("DeleteAsync: Deleting standard with ID: {StandardId}", id);
            try
            {
                var standard = await _repository.GetByIdAsync(id);
                if (standard == null)
                {
                    _logger.LogWarning("Standard with ID {StandardId} not found for deletion", id);
                    throw new ArgumentException($"Standard with ID {id} not found");
                }
                await _repository.DeleteAsync(id);
                _logger.LogInformation("DeleteAsync: Standard deleted with ID: {StandardId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete standard with ID: {StandardId}", id);
                throw;
            }
        }

        public async Task<List<StandardResource>> GetByCourseIdAsync(int courseId, int? districtId = null)
        {
            _logger.LogInformation("GetByCourseIdAsync: Fetching standards by Course ID: {CourseId}, District ID: {DistrictId}", courseId, districtId);
            try
            {
                var query = _repository.GetByCourseId(courseId);
                if (districtId.HasValue)
                {
                    query = query.Where(s => s.DistrictId == districtId.Value);
                }
                var standards = await query.ToListAsync();
                _logger.LogInformation("GetByCourseIdAsync: Found {Count} standards for Course ID: {CourseId}", standards.Count, courseId);
                return _mapper.Map<List<StandardResource>>(standards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch standards for Course ID: {CourseId}", courseId);
                throw;
            }
        }
    }
}