
using e_learning.Models;

namespace e_learning.models {
public class Review
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }
}

}