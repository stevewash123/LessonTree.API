using LessonTree.DAL.Domain;

namespace LessonTree.Service.Service.SystemConfig
{
    public interface ISystemConfigService
    {
        Task<string?> GetConfigValueAsync(string key);
        Task SetConfigValueAsync(string key, string value, string? description = null);
        Task<DateTime?> GetLastSeedDateAsync();
        Task SetLastSeedDateAsync(DateTime seedDate);
        Task<bool> ShouldReseedAsync();
    }
}