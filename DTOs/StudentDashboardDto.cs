namespace e_learning.DTOs
{
    public class StudentDashboardDto
    {
        public int EnrolledCourses { get; set; }
        public decimal TotalPaid { get; set; }
        public int CompletedCourses { get; set; }
        public int TotalQuizAttempts { get; set; }
        public double AverageQuizScore { get; set; }


        public IEnumerable<object> Courses { get; set; } = new List<object>();
    }
}
