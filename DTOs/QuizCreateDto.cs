using System.ComponentModel.DataAnnotations;
using e_learning.DTOs;

public class CreateQuizDto
{
    [Required(ErrorMessage = "عنوان الكويز مطلوب")]
    [StringLength(200, ErrorMessage = "العنوان يجب أن لا يتجاوز 200 حرف")]
    public string Title { get; set; }

    [StringLength(500, ErrorMessage = "الوصف يجب أن لا يتجاوز 500 حرف")]
    public string Description { get; set; }

    [Range(0, 100, ErrorMessage = "النسبة المطلوبة يجب أن تكون بين 0 و 100")]
    public int PassingScore { get; set; } = 70;

    public bool IsMandatory { get; set; } = true;
    public int MaxAttempts { get; set; } = 3;
    public int TimeLimitMinutes { get; set; } = 30;

    [Required(ErrorMessage = "الأسئلة مطلوبة")]
    [MinLength(1, ErrorMessage = "يجب إضافة سؤال واحد على الأقل")]
    public List<QuestionDto> Questions { get; set; }
}