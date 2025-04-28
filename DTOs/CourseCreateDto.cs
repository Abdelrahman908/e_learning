namespace e_learning.DTOs
{
    namespace e_learning.DTOs
    {
        public class CourseCreateDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public decimal Price { get; set; }

            // ✅ صورة رمزية للكورس
            public string? ThumbnailUrl { get; set; }

            // ✅ مدة الكورس (بالدقائق أو بالساعات)
            public int? DurationInMinutes { get; set; }

            // ✅ الفئة (تصنيف الكورس)
            public string? Category { get; set; }

            // ✅ مستوى الكورس (مبتدئ، متوسط، متقدم)
            public string? Level { get; set; }
            public IFormFile? Image { get; set; }


            // ✅ هل الكورس مفعل عند الإنشاء؟
            public bool IsActive { get; set; } = true;



        }
    }

}
