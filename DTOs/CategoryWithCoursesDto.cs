namespace e_learning.DTOs
{
    public class CategoryWithCoursesDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<CourseSimpleDto> Courses { get; set; } = new();
    }
}
