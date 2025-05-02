namespace e_learning.DTOs
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Message { get; set; }
        public DateTime? ExpiresIn { get; set; }
        public bool Success { get; set; }
        public UserResponseDto User { get; set; }
    }

}
