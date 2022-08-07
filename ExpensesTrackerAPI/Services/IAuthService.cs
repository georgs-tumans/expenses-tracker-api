using ExpensesTrackerAPI.Models.Database;

namespace ExpensesTrackerAPI.Helpers
{
    public interface IAuthService
    {
        public void HashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt);
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        public bool IsValidPassword(string password);
        public string CreateToken(User user, string tokenKey, UserType userTYpe);
        public void SendConfirmationEmail(string confirmationToken, string confirmationLink, User user);
    }
}
