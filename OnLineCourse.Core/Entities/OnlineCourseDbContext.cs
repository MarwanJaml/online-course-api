using Microsoft.EntityFrameworkCore;
using OnLineCourse.Core.Entities;

namespace OnLineCourse.Core.Entities
{
    public class OnlineCourseDbContext : DbContext
    {
        public OnlineCourseDbContext(DbContextOptions<OnlineCourseDbContext> options)
            : base(options)
        {
        }

        // DbSets with more concise declaration
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();
        public DbSet<Instructor> Instructors => Set<Instructor>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<SessionDetail> SessionDetails => Set<SessionDetail>();
        public DbSet<SmartApp> SmartApps => Set<SmartApp>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<VideoRequest> VideoRequests => Set<VideoRequest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Course Configuration
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.InstructorId);
            });

            // CourseCategory Configuration
            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
            });

            // Enrollment Configuration
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId);
                entity.Property(e => e.EnrollmentDate)
                      .HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.UserId);
            });

            // Instructor Configuration
            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.HasKey(e => e.InstructorId);
                entity.HasIndex(e => e.UserId);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.PaymentDate)
                      .HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.EnrollmentId);
            });

            // Review Configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId);
                entity.Property(e => e.ReviewDate)
                      .HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.UserId);
            });

            // Role Configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
            });

            // SessionDetail Configuration
            modelBuilder.Entity<SessionDetail>(entity =>
            {
                entity.HasKey(e => e.SessionId);
                entity.HasIndex(e => e.CourseId);
            });

            // UserProfile Configuration
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.DisplayName)
                      .HasDefaultValue("Guest");
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // UserRole Configuration
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.UserRoleId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.RoleId);
                entity.HasIndex(e => e.SmartAppId);
            });

            // VideoRequest Configuration
            modelBuilder.Entity<VideoRequest>(entity =>
            {
                entity.HasKey(e => e.VideoRequestId);
                entity.HasIndex(e => e.UserId);
            });
        }
    }
}