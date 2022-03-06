using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Database
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}