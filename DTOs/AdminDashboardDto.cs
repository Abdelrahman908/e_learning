namespace e_learning.DTOs
{
    public class AdminDashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalInstructors { get; set; }
        public int TotalCourses { get; set; }
        public int ActiveCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<CourseStatsDto> TopCoursesByRevenue { get; set; } = new();
        public List<CourseStatsDto> TopCoursesByRating { get; set; } = new();
    }

    public class CourseStatsDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = "";
        public int Enrollments { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }
}
