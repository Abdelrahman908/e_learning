public class Choice
{
    public int Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; } // هل هذا الاختيار صحيح؟

    // العلاقات
    public int QuestionId { get; set; }
    public Question Question { get; set; }
}