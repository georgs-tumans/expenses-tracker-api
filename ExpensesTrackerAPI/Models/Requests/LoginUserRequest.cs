using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class LoginUserRequest
    {
        /// <summary>
        /// Either user name or email
        /// </summary>
        [Required]
        public string AuthString { get; set; } = string.Empty;
        [Required]
        /// <summary>
        /// Account password
        /// </summary>
        public string Password { get; set; } = String.Empty;
    }
}
