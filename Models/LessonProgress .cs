using e_learning.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class LessonProgress
{
    public int Id { get; set; }

    // غيّر النوع من string إلى int
    [ForeignKey("User")]
    public int UserId { get; set; }

    public virtual User User { get; set; }

    public bool IsCompleted { get; set; } // هل أكمل الدرس؟
    public int ProgressPercentage { get; set; } // 0-100%
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan TimeSpent { get; set; } // الوقت المستغرق في الدرس

    // العلاقات
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; }
}
