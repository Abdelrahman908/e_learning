namespace e_learning.DTOs
{
    public class InstructorDashboardDto
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalEarnings { get; set; }
        public double? AverageRating { get; set; }
        public int CoursesCount { get; set; }
    }
}
