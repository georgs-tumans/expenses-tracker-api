using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Database
{
    public class User
    {
        [Required]
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public byte[] PasswordHash { get; set; }
        [Required]
        public byte[] PasswordSalt { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        [Required]
        public int Active { get; set; } = 1;
        [Required]
        public int AccountType { get; set; } = (int)UserType.User;
        [Required]
        public DateTime RegistrationDate { get; set; }
    }
}
