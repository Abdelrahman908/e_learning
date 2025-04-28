namespace e_learning.DTOs
{
    public class MyCourseDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
        public string InstructorName { get; set; } = "";
        public DateTime EnrolledAt { get; set; }
    }

}
