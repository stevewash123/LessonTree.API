using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LessonTree.DAL
{
    public class LessonTreeContextFactory : IDesignTimeDbContextFactory<LessonTreeContext>
    {
        public LessonTreeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LessonTreeContext>();
            optionsBuilder.UseSqlite("Data Source=LessonTree.db"); // Match your connection string
            return new LessonTreeContext(optionsBuilder.Options);
        }
    }
}