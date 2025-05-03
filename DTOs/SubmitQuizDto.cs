using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class SubmitQuizDto
    {
        [Required(ErrorMessage = "الإجابات مطلوبة")]
        public Dictionary<int, string> Answers { get; set; }
        // Key: QuestionId
        // Value: 
        //   - For MultipleChoice: ChoiceId as string
        //   - For Text: Answer text
    }



}
