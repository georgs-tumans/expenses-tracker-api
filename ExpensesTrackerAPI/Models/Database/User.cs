using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Database
{
    public class User
    {
        [Required]
        public int Id { get; set; }
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
        public int Active { get; set; } = 0;
    }
}
