using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using LessonTree.DAL.Domain;
using Microsoft.AspNetCore.Identity;

namespace LessonTree.DAL
{
    public class LessonTreeContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public LessonTreeContext(DbContextOptions<LessonTreeContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // LessonStandard many-to-many
            modelBuilder.Entity<LessonStandard>()
                .HasKey(ls => new { ls.LessonId, ls.StandardId });

            modelBuilder.Entity<LessonStandard>()
                .HasOne(ls => ls.Lesson)
                .WithMany(l => l.LessonStandards)
                .HasForeignKey(ls => ls.LessonId);

            modelBuilder.Entity<LessonStandard>()
                .HasOne(ls => ls.Standard)
                .WithMany(s => s.LessonStandards)
                .HasForeignKey(ls => ls.StandardId);

            
            modelBuilder.Entity<LessonDocument>()
                .HasKey(ld => new { ld.LessonId, ld.DocumentId });

            modelBuilder.Entity<LessonDocument>()
                .HasOne(ld => ld.Lesson)
                .WithMany(l => l.LessonDocuments)
                .HasForeignKey(ld => ld.LessonId);

            modelBuilder.Entity<LessonDocument>()
                .HasOne(ld => ld.Document)
                .WithMany(d => d.LessonDocuments)
                .HasForeignKey(ld => ld.DocumentId);
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<SubTopic> SubTopics { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Standard> Standards { get; set; }
        public DbSet<LessonStandard> LessonStandards { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<LessonDocument> LessonDocuments { get; set; }
    }
}