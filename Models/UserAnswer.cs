namespace e_learning.Models
{
    public class UserAnswer
    {
        public int Id { get; set; }
        public string AnswerText { get; set; } // للإجابات النصية
        public bool IsCorrect { get; set; } // هل كانت الإجابة صحيحة؟

        // العلاقات
        public int? ChoiceId { get; set; } // للإجابات الاختيارية
        public Choice Choice { get; set; }

        public int QuestionId { get; set; }
        public Question Question { get; set; }

        public int QuizResultId { get; set; }
        public QuizResult QuizResult { get; set; }
        public Answer Answer { get; set; }
        public int AnswerId { get; set; }

    }
}
