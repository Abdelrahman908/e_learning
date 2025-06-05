namespace e_learning.DTOs
{
    public class CourseSimpleDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }

        public bool IsFree => Price == 0;
    }

}
