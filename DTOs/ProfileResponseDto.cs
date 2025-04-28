// DTOs/ProfileResponseDto.cs
namespace e_learning.DTOs
{
    public class ProfileResponseDto
    {
        public int Id { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
    }
}
