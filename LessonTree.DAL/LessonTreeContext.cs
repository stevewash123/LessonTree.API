// **CORRECTED** - LessonTreeContext.cs with clean ScheduleConfiguration separation
// RESPONSIBILITY: Database context with proper ScheduleConfiguration domain
// DOES NOT: Reference old UserConfiguration period assignments (removed)
// CALLED BY: Entity Framework for all database operations

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Query;
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
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonStandard>()
                .HasOne(ls => ls.Standard)
                .WithMany(s => s.LessonStandards)
                .OnDelete(DeleteBehavior.Cascade);

            // LessonAttachment many-to-many
            modelBuilder.Entity<LessonAttachment>()
                .HasKey(ld => new { ld.LessonId, ld.AttachmentId });

            modelBuilder.Entity<LessonAttachment>()
                .HasOne(ld => ld.Lesson)
                .WithMany(l => l.LessonAttachments)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonAttachment>()
                .HasOne(ld => ld.Attachment)
                .WithMany(d => d.LessonAttachments)
                .OnDelete(DeleteBehavior.Cascade);

            // Hierarchy relationships with cascade delete
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SubTopic>()
                .HasOne(st => st.Topic)
                .WithMany(t => t.SubTopics)
                .HasForeignKey(st => st.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Topic)
                .WithMany(t => t.Lessons)
                .HasForeignKey(l => l.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.SubTopic)
                .WithMany(st => st.Lessons)
                .HasForeignKey(l => l.SubTopicId)
                .OnDelete(DeleteBehavior.Cascade);

            // Organizational hierarchy relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.District)
                .WithMany(d => d.Staff)
                .HasForeignKey(u => u.DistrictId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.School)
                .WithMany(s => s.Teachers)
                .HasForeignKey(u => u.SchoolId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<School>()
                .HasOne(s => s.District)
                .WithMany(d => d.Schools)
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.School)
                .WithMany(s => s.Departments)
                .HasForeignKey(d => d.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-Department many-to-many
            modelBuilder.Entity<User>()
                .HasMany(u => u.Departments)
                .WithMany(d => d.Members)
                .UsingEntity(j => j.ToTable("UserDepartments"));

            modelBuilder.Entity<Standard>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Standards)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Standard>()
                .HasOne(s => s.Topic)
                .WithMany(c => c.Standards)
                .HasForeignKey(s => s.TopicId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Standard>()
                .HasOne(s => s.District)
                .WithMany(d => d.Standards)
                .HasForeignKey(s => s.DistrictId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserConfiguration - simplified, no period assignments
            modelBuilder.Entity<UserConfiguration>()
                .HasOne(uc => uc.User)
                .WithOne(u => u.Configuration)
                .HasForeignKey<UserConfiguration>(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserConfiguration>()
                .HasIndex(uc => uc.UserId)
                .IsUnique();

            // ScheduleConfiguration
            modelBuilder.Entity<ScheduleConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
                entity.Property(e => e.SchoolYear).HasMaxLength(20).IsRequired();
                entity.Property(e => e.TeachingDays).HasMaxLength(200).IsRequired();
                entity.Property(e => e.PeriodsPerDay).IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.Status });
                entity.HasIndex(e => new { e.UserId, e.SchoolYear });
            });

            // PeriodAssignment (belongs to ScheduleConfiguration)
            modelBuilder.Entity<PeriodAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Period).IsRequired();
                entity.Property(e => e.SpecialPeriodType).HasMaxLength(50);
                entity.Property(e => e.TeachingDays).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Room).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.BackgroundColor).HasMaxLength(7).IsRequired();
                entity.Property(e => e.FontColor).HasMaxLength(7).IsRequired();

                entity.HasOne(e => e.ScheduleConfiguration)
                      .WithMany(sc => sc.PeriodAssignments)
                      .HasForeignKey(e => e.ScheduleConfigurationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ScheduleConfigurationId, e.Period })
                      .IsUnique();

                entity.ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_ExclusiveAssignment",
                    "(CourseId IS NOT NULL AND SpecialPeriodType IS NULL) OR (CourseId IS NULL AND SpecialPeriodType IS NOT NULL)"));

                entity.ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_CourseId_Positive",
                    "CourseId IS NULL OR CourseId > 0"));

                entity.ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_TeachingDays_NotEmpty",
                    "TeachingDays IS NOT NULL AND LENGTH(TRIM(TeachingDays)) > 0"));

                entity.ToTable(t => t.HasCheckConstraint("CK_PeriodAssignment_TeachingDays_ValidDays",
                    @"TeachingDays NOT LIKE '%[^MondayTueswdhFrig,]%' AND 
                        (TeachingDays LIKE '%Monday%' OR 
                        TeachingDays LIKE '%Tuesday%' OR 
                        TeachingDays LIKE '%Wednesday%' OR 
                        TeachingDays LIKE '%Thursday%' OR 
                        TeachingDays LIKE '%Friday%' OR
                        TeachingDays LIKE '%Saturday%' OR
                        TeachingDays LIKE '%Sunday%')"));
            });

            // Schedule with ScheduleConfiguration relationship
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.ScheduleConfiguration)
                .WithMany(sc => sc.Schedules)
                .HasForeignKey(s => s.ScheduleConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);

            // SpecialDay relationship with Schedule
            modelBuilder.Entity<SpecialDay>()
                .HasOne(sd => sd.Schedule)
                .WithMany(s => s.SpecialDays)
                .HasForeignKey(sd => sd.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SpecialDay>()
                .HasIndex(sd => new { sd.ScheduleId, sd.Date })
                .HasDatabaseName("IX_SpecialDays_Schedule_Date");

            // ✅ FIX: Clean ScheduleEvent configuration to eliminate LessonId1 shadow property
            modelBuilder.Entity<ScheduleEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ CRITICAL: Explicit Lesson relationship configuration - this prevents LessonId1
                entity.HasOne(e => e.Lesson)
                      .WithMany() // Lesson doesn't have a ScheduleEvents collection navigation
                      .HasForeignKey(e => e.LessonId)
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                // Schedule relationship
                entity.HasOne(e => e.Schedule)
                      .WithMany(s => s.ScheduleEvents)
                      .HasForeignKey(e => e.ScheduleId)
                      .OnDelete(DeleteBehavior.Cascade);

                // ✅ IMPORTANT: Explicitly state CourseId is NOT a foreign key
                entity.Property(e => e.CourseId)
                      .IsRequired(false);
                // DO NOT add HasOne/WithMany for Course - CourseId is just a data field

                // Property configurations
                entity.Property(e => e.LessonId).IsRequired(false);
                entity.Property(e => e.ScheduleId).IsRequired(true);
                entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.EventCategory).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Comment).HasMaxLength(1000).IsRequired(false);
                entity.Property(e => e.ScheduleSort).IsRequired(true);

                // Indexes
                entity.HasIndex(e => new { e.ScheduleId, e.Date, e.Period })
                      .IsUnique()
                      .HasDatabaseName("IX_ScheduleEvents_Schedule_Date_Period");

                entity.HasIndex(e => e.LessonId)
                      .HasDatabaseName("IX_ScheduleEvents_LessonId");

                // ✅ CRITICAL FIX: SpecialDay relationship - use SetNull to prevent cascade deletion of SpecialDays
                entity.HasOne(e => e.SpecialDay)
                      .WithMany() // SpecialDay doesn't have a ScheduleEvents collection navigation
                      .HasForeignKey(e => e.SpecialDayId)
                      .OnDelete(DeleteBehavior.SetNull) // Changed from Cascade to SetNull to preserve SpecialDays
                      .IsRequired(false);

                // ✅ ADD: Performance indexes for lesson queries
                entity.HasIndex(e => new { e.ScheduleId, e.LessonId })
                      .HasDatabaseName("IX_ScheduleEvents_Schedule_Lesson");
            });

            // ✅ ADD: Performance indexes for lesson hierarchy
            modelBuilder.Entity<Lesson>(entity =>
            {
                // Existing lesson configuration...
                entity.HasIndex(l => new { l.UserId, l.TopicId, l.SubTopicId })
                      .HasDatabaseName("IX_Lessons_UserId_TopicId_SubTopicId");

                entity.HasIndex(l => new { l.TopicId, l.SubTopicId, l.SortOrder })
                      .HasDatabaseName("IX_Lessons_Container_SortOrder");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasIndex(t => new { t.CourseId, t.SortOrder })
                      .HasDatabaseName("IX_Topics_Course_SortOrder");
            });

            modelBuilder.Entity<SubTopic>(entity =>
            {
                entity.HasIndex(st => new { st.TopicId, st.SortOrder })
                      .HasDatabaseName("IX_SubTopics_Topic_SortOrder");
            });
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
        public DbSet<SpecialDay> SpecialDays { get; set; }
        public DbSet<ScheduleConfiguration> ScheduleConfigurations { get; set; }
        public DbSet<PeriodAssignment> PeriodAssignments { get; set; }
    }
}