namespace e_learning.DTOs
{
    namespace e_learning.DTOs
    {
        public class CourseFilterDto
        {
            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }
            public double? MinRating { get; set; }

            // Pagination
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
    }

}
