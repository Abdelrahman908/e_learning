using System.ComponentModel.DataAnnotations;
using ExpressiveAnnotations.Attributes;
namespace e_learning.DTOs
{
    public class QuestionDto
    {
        [Required(ErrorMessage = "نص السؤال مطلوب")]
        [StringLength(500, ErrorMessage = "السؤال يجب أن لا يتجاوز 500 حرف")]
        public string Text { get; set; }

        [Required(ErrorMessage = "نوع السؤال مطلوب")]
        public QuestionType Type { get; set; }

        [Range(1, 10, ErrorMessage = "النقاط يجب أن تكون بين 1 و 10")]
        public int Points { get; set; } = 1;

        [RequiredIf("Type", QuestionType.MultipleChoice, ErrorMessage = "الخيارات مطلوبة للأسئلة الاختيارية")]
        public List<ChoiceDto> Choices { get; set; }
    }
}
