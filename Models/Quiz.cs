namespace e_learning.Models
{

    public class Quiz
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();

        public ICollection<Question> Questions { get; set; }
    }


}
