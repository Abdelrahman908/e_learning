namespace e_learning.DTOs.Responses
{
    public class RegisterResponse
    {
        public int UserId { get; set; }  // تغيير من Guid إلى int
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresIn { get; set; }
        public UserInfoDto User { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserInfoDto
    {
        public int Id { get; set; }  // تغيير من Guid إلى int
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}