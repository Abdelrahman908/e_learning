using Microsoft.EntityFrameworkCore;
using e_learning.Models;
using e_learning.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using e_learning.Models.e_learning.Models;
using e_learning.models;

namespace e_learning.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // الجداول الأساسية
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonMaterial> LessonMaterials { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // جداول الاختبارات
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        // جداول التقدم
        public DbSet<LessonProgress> LessonProgresses { get; set; }

        // جداول أخرى
        public DbSet<Message> Messages { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<EmailConfirmationCode> EmailConfirmationCodes { get; set; }
        public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Decimal Precision Settings
            modelBuilder.Entity<Course>()
                .Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // 2. Core Relationships Configuration
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fixed Course-Category relationship with proper nullability
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .IsRequired(false)  // Explicitly mark as optional
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Lessons and Learning Materials Configuration
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Quiz)
                .WithOne(q => q.Lesson)
                .HasForeignKey<Quiz>(q => q.LessonId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Lesson>()
                .Property(l => l.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<LessonMaterial>()
                .HasOne(m => m.Lesson)
                .WithMany(l => l.Materials)
                .HasForeignKey(m => m.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonMaterial>()
                .Property(m => m.FileSize)
                .HasColumnType("bigint");

            // 4. Quizzes and Questions Configuration (Updated)
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId);


            modelBuilder.Entity<Quiz>()
                .Property(q => q.PassingScore)
                .HasDefaultValue(70);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .Property(a => a.IsCorrect)
                .HasDefaultValue(false);

            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.Quiz)
                .WithMany(q => q.Results)
                .HasForeignKey(qr => qr.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.User)
                .WithMany(u => u.QuizResults)
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizResult>()
                .Property(qr => qr.Score)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.QuizResult)
                .WithMany(qr => qr.UserAnswers)
                .HasForeignKey(ua => ua.QuizResultId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Answer)
                .WithMany()
                .HasForeignKey(ua => ua.AnswerId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Notifications Configuration
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.ReceivedNotifications)  // خاصية جديدة في User
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(u => u.SentNotifications)  // خاصية جديدة في User
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.Restrict);


            // 6. User Progress Tracking (Updated)
            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.Lesson)
                .WithMany(l => l.Progresses)
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonProgress>()
                .Property(lp => lp.IsCompleted)
                .HasDefaultValue(false);

            modelBuilder.Entity<LessonProgress>()
                .Property(lp => lp.ProgressPercentage)
                .HasDefaultValue(0);

            // 7. Helper Tables Configuration
            modelBuilder.Entity<EmailConfirmationCode>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique(false);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(6);
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<PasswordResetCode>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique(false);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(6);
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // UserRefreshToken Configuration
            modelBuilder.Entity<UserRefreshToken>()
                .HasIndex(e => e.RefreshToken)
                .IsUnique();

            modelBuilder.Entity<UserRefreshToken>()
                .Property(e => e.ExpiryDate)
                .IsRequired();

            modelBuilder.Entity<UserRefreshToken>()
                .Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<UserRefreshToken>()
                .HasOne(urt => urt.User)
                .WithMany()
                .HasForeignKey(urt => urt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 8. Indexes and Performance Optimization
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.Title)
                .IsUnique(false);

            modelBuilder.Entity<Lesson>()
                .HasIndex(l => new { l.CourseId, l.Order })
                .IsUnique();

            modelBuilder.Entity<QuizResult>()
                .HasIndex(qr => new { qr.QuizId, qr.UserId, qr.CompletedAt })
                .IsUnique(false);

            // 9. Default Values and Timestamps
            modelBuilder.Entity<Course>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Lesson>()
                .Property(l => l.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Quiz>()
                .Property(q => q.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Additional optimization for frequently queried fields
            modelBuilder.Entity<Course>()
                .HasIndex(c => c.IsActive);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.IsActive);

            //modelBuilder.Entity<Lesson>()
            //    .HasIndex(l => l.IsActive);
        }
    }
}