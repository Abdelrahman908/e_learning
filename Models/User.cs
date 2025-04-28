using e_learning.models;


namespace e_learning.Models

{

    public class User
    {
        public int Id { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; }
        public Profile Profile { get; set; }

        public bool IsEmailConfirmed { get; set; } = false; // ✅ الإضافة الجديدة

        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<Course>? Courses { get; set; } // لو Instructor
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

}