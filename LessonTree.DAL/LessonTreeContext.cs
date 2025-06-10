// Full File: LessonTree.DAL/LessonTreeContext.cs
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

            // New hierarchy relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.District)
                .WithMany(d => d.Staff)
                .HasForeignKey(u => u.DistrictId)
                .OnDelete(DeleteBehavior.SetNull); // Optional: Set null if District is deleted

            modelBuilder.Entity<User>()
                .HasOne(u => u.School)
                .WithMany(s => s.Teachers)
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.SetNull); // Optional: Set null if School is deleted

            modelBuilder.Entity<School>()
                .HasOne(s => s.District)
                .WithMany(d => d.Schools)
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.Cascade); // District -> School

            modelBuilder.Entity<Department>()
                .HasOne(d => d.School)
                .WithMany(s => s.Departments)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.Cascade); // School -> Department

            // User-Department many-to-many
            modelBuilder.Entity<User>()
                .HasMany(u => u.Departments)
                .WithMany(d => d.Members)
                .UsingEntity(j => j.ToTable("UserDepartments"));

            modelBuilder.Entity<Standard>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Standards) // Assuming Course will have a Standards collection
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete when Course is deleted

            modelBuilder.Entity<Standard>()
                .HasOne(s => s.Topic)
                .WithMany(c => c.Standards) // Assuming Course will have a Standards collection
                .HasForeignKey(s => s.TopicId)
                .OnDelete(DeleteBehavior.SetNull); // Cascade delete when Course is deleted

            modelBuilder.Entity<Standard>()
                .HasOne(s => s.District)
                .WithMany(d => d.Standards) // Assuming District will have a Standards collection
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PeriodAssignment>()
                .HasOne(pa => pa.UserConfiguration)
                .WithMany(uc => uc.PeriodAssignments)
                .HasForeignKey(pa => pa.UserConfigurationId)
                .OnDelete(DeleteBehavior.Cascade); // Delete assignments when user config is deleted

            // Ensure exactly one of CourseId or SpecialPeriodType is specified (exclusive assignment)
            modelBuilder.Entity<PeriodAssignment>()
                .ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_ExclusiveAssignment",
                    "(CourseId IS NOT NULL AND SpecialPeriodType IS NULL) OR (CourseId IS NULL AND SpecialPeriodType IS NOT NULL)"));

            // Ensure CourseId is positive when specified
            modelBuilder.Entity<PeriodAssignment>()
                .ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_CourseId_Positive",
                    "CourseId IS NULL OR CourseId > 0"));

            // Add constraint to ensure TeachingDays is not empty for PeriodAssignments
            modelBuilder.Entity<PeriodAssignment>()
                .ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_TeachingDays_NotEmpty",
                    "TeachingDays IS NOT NULL AND LENGTH(TRIM(TeachingDays)) > 0"));

            // Add constraint to ensure valid day names in TeachingDays
            modelBuilder.Entity<PeriodAssignment>()
                .ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_TeachingDays_ValidDays",
                    @"TeachingDays NOT LIKE '%[^MondayTueswdhFrig,]%' AND 
                        (TeachingDays LIKE '%Monday%' OR 
                        TeachingDays LIKE '%Tuesday%' OR 
                        TeachingDays LIKE '%Wednesday%' OR 
                        TeachingDays LIKE '%Thursday%' OR 
                        TeachingDays LIKE '%Friday%' OR
                        TeachingDays LIKE '%Saturday%' OR
                        TeachingDays LIKE '%Sunday%')"));

            modelBuilder.Entity<UserConfiguration>()
                .HasOne(uc => uc.User)
                .WithOne(u => u.Configuration) // FIXED: Specify the navigation property
                .HasForeignKey<UserConfiguration>(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete config when user is deleted

            // Add indexes for performance
            modelBuilder.Entity<UserConfiguration>()
                .HasIndex(uc => uc.UserId)
                .IsUnique(); // One configuration per user

            modelBuilder.Entity<PeriodAssignment>()
                .HasIndex(pa => new { pa.UserConfigurationId, pa.Period, pa.TeachingDays })
                .HasDatabaseName("IX_PeriodAssignments_UserConfig_Period_TeachingDays");
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<SubTopic> SubTopics { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Standard> Standards { get; set; }
        public DbSet<LessonStandard> LessonStandards { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<LessonAttachment> LessonAttachments { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleEvent> ScheduleEvents { get; set; }
        public DbSet<UserConfiguration> UserConfigurations { get; set; }
        public DbSet<PeriodAssignment> PeriodAssignments { get; set; }
    }
}