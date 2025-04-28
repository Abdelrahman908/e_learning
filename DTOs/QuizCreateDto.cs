namespace e_learning.DTOs
{
    public class QuizCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public List<QuestionCreateDto> Questions { get; set; } = new();
    }

    public class QuestionCreateDto
    {
        public string Text { get; set; } = string.Empty;
        public List<ChoiceCreateDto> Choices { get; set; } = new();
    }

    public class ChoiceCreateDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
