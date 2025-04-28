namespace e_learning.Models
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }

        public User User { get; set; } // العلاقة مع جدول الـ Users
    }
}
