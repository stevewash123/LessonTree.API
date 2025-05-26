using LessonTree.DAL;
using LessonTree.DAL.Repositories;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using LessonTree.DAL.Domain;

namespace LessonTree.BLL.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repository, ILogger<UserService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public User GetById(int id)
        {
            _logger.LogDebug("Fetching user by ID: {UserId}", id);
            var user = _repository.GetById(id);
            if (user == null)
                _logger.LogWarning("User with ID {UserId} not found in service", id);
            return user;
        }

        public User GetByUserName(string userName)
        {
            _logger.LogDebug("Fetching user by UserName: {UserName}", userName);
            var user = _repository.GetByUserName(userName);
            if (user == null)
                _logger.LogWarning("User with UserName {UserName} not found in service", userName);
            return user;
        }

        public List<User> GetAll()
        {
            _logger.LogDebug("Fetching all users");
            return _repository.GetAll();
        }

        public void Add(User user)
        {
            _logger.LogDebug("Adding user: {UserName}", user.UserName);
            _repository.Add(user);
            _logger.LogInformation("User added with ID: {UserId}", user.Id);
        }

        public User Update(User user)
        {
            _logger.LogDebug("Updating user: {UserName}", user.UserName);
            _repository.Update(user);
            _logger.LogInformation("User updated with ID: {UserId}", user.Id);

            // Return the updated entity
            return _repository.GetById(user.Id);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting user with ID: {UserId}", id);
            _repository.Delete(id);
            _logger.LogInformation("User deleted with ID: {UserId}", id);
        }
    }
}