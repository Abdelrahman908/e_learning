namespace e_learning.Services
{
    public interface IPasswordValidator
    {
        bool Validate(string password, out string errorMessage);
        bool IsStrongPassword(string password);
    }
}