using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class RegisterUserRequest
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9](?:[a-zA-Z0-9.,'_ -]*[a-zA-Z0-9])?$", ErrorMessage = "Username can only contain letters and numbers")]
        [StringLength(maximumLength: 15, MinimumLength = 2)]
        public string UserName { get; set; } = string.Empty;
        [Required]
        [StringLength(maximumLength: 32, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [StringLength(maximumLength: 32, MinimumLength = 6)]
        public string RepeatedPassword { get; set; } = string.Empty;
        [StringLength(maximumLength: 30, MinimumLength = 2)]
        [RegularExpression("^[a-zA-Z](?:[a-zA-Z.,' -]*[a-zA-Z])?$", ErrorMessage = "The only special characters allowed for the name are ',.-'")]
        public string? Name { get; set; }
        [StringLength(maximumLength: 30, MinimumLength = 2)]
        [RegularExpression("^[a-zA-Z](?:[a-zA-Z.,' -]*[a-zA-Z])?$", ErrorMessage = "The only special characters allowed for the surname are ',.-'")]
        public string? Surname { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
