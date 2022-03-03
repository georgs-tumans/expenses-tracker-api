using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public double Amount { get; set; }

        public string? Note { get; set; }
    }
}