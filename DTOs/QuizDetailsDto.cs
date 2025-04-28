using e_learning.DTOs;

public class QuizDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<QuestionForStudentDto> Questions { get; set; } = new();
}
