namespace e_learning.DTOs
{
    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string UserAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
    }
}
