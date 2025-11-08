using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LessonTree.DAL
{
    public class LessonTreeContextFactory : IDesignTimeDbContextFactory<LessonTreeContext>
    {
        public LessonTreeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LessonTreeContext>();
            optionsBuilder.UseNpgsql("Host=ep-misty-art-a4l8e5z4-pooler.us-east-1.aws.neon.tech;Port=5432;Database=lessontree_db;Username=neondb_owner;Password=npg_WBIEVn6OfuH5;SSL Mode=Require;");
            return new LessonTreeContext(optionsBuilder.Options);
        }
    }
}