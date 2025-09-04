using AutoMapper;
using LessonTree.BLL.Services;
using Microsoft.Extensions.Logging;

namespace LessonTree.Tests.Helpers
{
    /// <summary>
    /// Base class for all test classes providing common setup and utilities
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        protected readonly IMapper Mapper;
        protected readonly ILoggerFactory LoggerFactory;
        
        protected TestBase()
        {
            // Setup AutoMapper with the actual mapping profile
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            Mapper = mapperConfig.CreateMapper();
            
            // Setup logger factory for testing
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => 
                builder.AddDebug().SetMinimumLevel(LogLevel.Debug));
        }
        
        /// <summary>
        /// Create a logger for a specific type
        /// </summary>
        /// <typeparam name="T">Type to create logger for</typeparam>
        /// <returns>Logger instance</returns>
        protected ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
        
        /// <summary>
        /// Generate a unique test user ID
        /// </summary>
        /// <returns>Unique user ID for testing</returns>
        protected int GetTestUserId() => new Random().Next(1000, 9999);
        
        /// <summary>
        /// Generate a test date within a reasonable range
        /// </summary>
        /// <returns>Test date</returns>
        protected DateTime GetTestDate() => DateTime.Now.AddDays(new Random().Next(-30, 365));
        
        public virtual void Dispose()
        {
            LoggerFactory?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}