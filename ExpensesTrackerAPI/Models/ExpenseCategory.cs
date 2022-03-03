using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models
{
    public class ExpenseCategory
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
    }
}
