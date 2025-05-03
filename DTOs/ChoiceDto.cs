using System.ComponentModel.DataAnnotations;

namespace e_learning.DTOs
{
    public class ChoiceDto
    {
        [Required(ErrorMessage = "نص الخيار مطلوب")]
        [StringLength(200, ErrorMessage = "الخيار يجب أن لا يتجاوز 200 حرف")]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }
    }
}
