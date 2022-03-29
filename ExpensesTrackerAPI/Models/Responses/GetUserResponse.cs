﻿namespace ExpensesTrackerAPI.Models.Responses
{
    public class GetUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int AccountType { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int Active { get; set; }
    }
}
