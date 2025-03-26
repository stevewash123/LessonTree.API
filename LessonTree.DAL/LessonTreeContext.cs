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
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete when Lesson is deleted

            modelBuilder.Entity<LessonStandard>()
                .HasOne(ls => ls.Standard)
                .WithMany(s => s.LessonStandards)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete when Standard is deleted

            // LessonAttachment many-to-many
            modelBuilder.Entity<LessonAttachment>()
                .HasKey(ld => new { ld.LessonId, ld.AttachmentId });

            modelBuilder.Entity<LessonAttachment>()
                .HasOne(ld => ld.Lesson)
                .WithMany(l => l.LessonAttachments)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete when Lesson is deleted

            modelBuilder.Entity<LessonAttachment>()
                .HasOne(ld => ld.Attachment)
                .WithMany(d => d.LessonAttachments)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete when Attachment is deleted

            // Hierarchy relationships with cascade delete
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Course -> Topic

            modelBuilder.Entity<SubTopic>()
                .HasOne(st => st.Topic)
                .WithMany(t => t.SubTopics)
                .HasForeignKey(st => st.TopicId)
                .OnDelete(DeleteBehavior.Cascade); // Topic -> SubTopic

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Topic)
                .WithMany(t => t.Lessons)
                .HasForeignKey(l => l.TopicId)
                .OnDelete(DeleteBehavior.Cascade); // Topic -> Lesson

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.SubTopic)
                .WithMany(st => st.Lessons)
                .HasForeignKey(l => l.SubTopicId)
                .OnDelete(DeleteBehavior.Cascade); // SubTopic -> Lesson
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<SubTopic> SubTopics { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Standard> Standards { get; set; }
        public DbSet<LessonStandard> LessonStandards { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<LessonAttachment> LessonAttachments { get; set; }
    }
}