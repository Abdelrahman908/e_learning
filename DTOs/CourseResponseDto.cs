namespace e_learning.DTOs
{
    public class CourseResponseDto
    {
        public int Id { get; set; }              // تغيير من Guid إلى int
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool? IsActive { get; set; }
        public int CategoryId { get; set; }      // تغيير من Guid إلى int
        public string? ImageUrl { get; set; }
        public int InstructorId { get; set; }    // تغيير من Guid إلى int
        public string? InstructorName { get; set; }
        public double? AverageRating { get; set; }
        public string? CategoryName { get; set; }
        public string? Level { get; set; }
        public int? DurationInMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}