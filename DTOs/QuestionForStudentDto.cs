namespace e_learning.DTOs
{
    public class QuestionForStudentDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public List<ChoiceForStudentDto> Choices { get; set; }
    }
}
