using Microsoft.EntityFrameworkCore;
using e_learning.Models;
using e_learning.models;

namespace e_learning.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User ↔ Profile
            modelBuilder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ Enrollment
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // User ↔ Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Instructor ↔ Course
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course ↔ Lesson
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Course)
                .WithMany(c => c.Lessons)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course ↔ Quiz
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz ↔ Question
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // Question ↔ Choice
            modelBuilder.Entity<Choice>()
                .HasOne(c => c.Question)
                .WithMany(q => q.Choices)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quiz ↔ QuizResult
            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.Quiz)
                .WithMany()
                .HasForeignKey(qr => qr.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ↔ QuizResult
            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.User)
                .WithMany()
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification ↔ User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment ↔ User
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment ↔ Course
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
