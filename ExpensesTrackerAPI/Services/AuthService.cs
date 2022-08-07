using ExpensesTrackerAPI.Models.Database;
using MailKit.Net.Smtp;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpensesTrackerAPI.Helpers
{
    public class AuthService : IAuthService
    {

        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void HashPassword(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var calculatedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return calculatedHash.SequenceEqual(passwordHash);
            }
        }

        public bool IsValidPassword(string password)
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            return hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) && hasSymbols.IsMatch(password);
        }

        public string CreateToken(User user, string tokenKey, UserType userType)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.PrimarySid, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (userType == UserType.User) claims.Add(new Claim(ClaimTypes.Role, "user"));
            else claims.Add(new Claim(ClaimTypes.Role, "admin"));

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(2), signingCredentials: cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public void SendConfirmationEmail(string confirmationToken, string confirmationLink, User user)
        {
            string senderEmail = _configuration.GetSection("SendingEmail").GetSection("SenderAddress").Value;
            string smtpServer = _configuration.GetSection("SendingEmail").GetSection("SmtpServer").Value;
            int smtpPort = Convert.ToInt32(_configuration.GetSection("SendingEmail").GetSection("SmtpPort").Value);
            string senderLoginPass = _configuration.GetSection("SendingEmail").GetSection("SenderLoginPassword").Value;

            StringBuilder text = new StringBuilder();
            text.Append("Hello!").Append(Environment.NewLine).Append("To activate your Expenses Tracker API account, please click on the following link:").Append(Environment.NewLine);
            text.Append(confirmationLink).Append(Environment.NewLine).Append(Environment.NewLine);
            text.Append($"The link expires in {Convert.ToInt32(_configuration.GetSection("TokenExpirationHours").Value)} hours.");

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("Expense Tracking API", senderEmail));
            mailMessage.To.Add(new MailboxAddress(user.Name is null ? "User" : user.Name, user.Email));
            mailMessage.Subject = "Account confirmation";
            mailMessage.Body = new TextPart("plain")
            {
                Text = text.ToString()
            };

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(smtpServer, smtpPort, true);
                smtpClient.Authenticate(senderEmail, senderLoginPass);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }
        }
    }
}
