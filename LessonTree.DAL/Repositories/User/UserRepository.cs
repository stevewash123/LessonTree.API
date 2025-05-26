using System.Collections.Generic;
using System.Linq;
using LessonTree.DAL.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LessonTree.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly LessonTreeContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(LessonTreeContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public User GetById(int id)
        {
            _logger.LogDebug("Retrieving user by ID: {UserId}", id);
            var user = _context.Users.Find(id);
            if (user == null)
                _logger.LogWarning("User with ID {UserId} not found", id);
            return user;
        }

        public User GetByUserName(string userName)
        {
            _logger.LogDebug("Retrieving user by UserName: {UserName}", userName);
            var user = _context.Users.SingleOrDefault(u => u.UserName == userName);
            if (user == null)
                _logger.LogWarning("User with UserName {UserName} not found", userName);
            return user;
        }

        public List<User> GetAll()
        {
            _logger.LogDebug("Retrieving all users");
            return _context.Users.ToList();
        }

        public void Add(User user)
        {
            _logger.LogDebug("Adding user: {UserName}", user.UserName);
            _context.Users.Add(user);
            _context.SaveChanges();
            _logger.LogInformation("Added user with ID: {UserId}, UserName: {UserName}", user.Id, user.UserName);
        }

        public void Update(User user)
        {
            _logger.LogDebug("Updating user: {UserName}", user.UserName);
            _context.Users.Update(user);
            _context.SaveChanges();
            _logger.LogInformation("Updated user with ID: {UserId}, UserName: {UserName}", user.Id, user.UserName);
        }

        public void Delete(int id)
        {
            _logger.LogDebug("Deleting user with ID: {UserId}", id);
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
                _logger.LogInformation("Deleted user with ID: {UserId}", id);
            }
            else
            {
                _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            }
        }
    }
}