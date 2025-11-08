using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LessonTree.DAL
{
    public class LessonTreeContextFactory : IDesignTimeDbContextFactory<LessonTreeContext>
    {
        public LessonTreeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LessonTreeContext>();
            optionsBuilder.UseNpgsql("Host=db.xrgfutvuqijtggtrjswx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=\"bVKvBTCjsON9Zn1O\";SSL Mode=Require;");
            return new LessonTreeContext(optionsBuilder.Options);
        }
    }
}