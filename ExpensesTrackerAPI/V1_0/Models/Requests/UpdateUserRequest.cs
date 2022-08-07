using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class UpdateUserRequest
    {
        [RegularExpression("^[a-zA-Z0-9](?:[a-zA-Z0-9.,'_ -]*[a-zA-Z0-9])?$", ErrorMessage = "Username can only contain letters and numbers")]
        public string? Username { get; set; } = string.Empty;
        [RegularExpression("^[a-zA-Z](?:[a-zA-Z.,' -]*[a-zA-Z])?$", ErrorMessage = "The only special characters allowed for the name are ',.-'")]
        public string? Name { get; set; } = string.Empty;
        [RegularExpression("^[a-zA-Z](?:[a-zA-Z.,' -]*[a-zA-Z])?$", ErrorMessage = "The only special characters allowed for the surname are ',.-'")]
        public string? Surname { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
