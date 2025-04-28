namespace e_learning.DTOs
{
    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; } // رابط الصورة



        public string? InstructorName { get; set; }
        public double? AverageRating { get; set; }

        public string? Category { get; set; }
        public string? Level { get; set; }
        public int? DurationInMinutes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
