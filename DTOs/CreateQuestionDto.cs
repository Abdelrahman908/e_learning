namespace e_learning.DTOs
{
    public class CreateQuestionDto
    {
        public string? Text { get; set; }
        public List<ChoiceDto> Choices { get; set; }
    }
}
