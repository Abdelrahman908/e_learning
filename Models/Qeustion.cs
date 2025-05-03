using e_learning.Models;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; }
    public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
    public int Points { get; set; } = 1; // عدد النقاط لكل سؤال

    // العلاقات
    public int QuizId { get; set; }
    public Quiz Quiz { get; set; }
    public List<Answer> Answers { get; set; } = new List<Answer>();

    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}

public enum QuestionType
{
    MultipleChoice, // اختيار من متعدد
    TrueFalse,      // صح أو خطأ
    Text            // إجابة نصية
}