using ExpensesTrackerAPI.Models.Database;

namespace ExpensesTrackerAPI.Helpers
{
    /// <summary>
    /// Provides necessary authentification logic
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Hashes a password
        /// </summary>
        /// <param name="password">The password string</param>
        /// <param name="passwordHash">Hashed password bytes</param>
        /// <param name="passwordSalt">Hashed password salt</param>
        /// <returns></returns>
        public void HashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt);
        /// <summary>
        /// Verifies a password hash
        /// </summary>
        /// <param name="password">The password string</param>
        /// <param name="passwordHash">Password hash bytes</param>
        /// <param name="passwordSalt">Password salt</param>
        /// <returns>Whether the hash is valid</returns>
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        /// <summary>
        /// Verifies whether a password matches all password requirements
        /// </summary>
        /// <param name="password">The password string</param>
        /// <returns>Whether the password matches all requirements</returns>
        public bool IsValidPassword(string password);
        /// <summary>
        /// Creates authentification token
        /// </summary>
        /// <param name="user">Current user object</param>
        /// <param name="tokenKey">Token key</param>
        /// <param name="userTYpe">Type of the user - admin or regular</param>
        /// <returns>The created token</returns>
        public string CreateToken(User user, string tokenKey, UserType userTYpe);
        /// <summary>
        /// Sends an account activation email
        /// </summary>
        /// <param name="confirmationToken">Account confirmation token</param>
        /// <param name="confirmationLink">Account confirmation link</param>
        /// <param name="user">New user object</param>
        /// <returns></returns>
        public void SendConfirmationEmail(string confirmationToken, string confirmationLink, User user);
    }
}
