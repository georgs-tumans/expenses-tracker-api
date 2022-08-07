using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Database
{
    public class ExpenseCategory
    {
        [Required]
        [Key]
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public int Active { get; set; } = 1;
        [Required]
        /// <summary>
        /// Every user has default pre-defined categories, that cannot be edited and are the same across all users
        /// </summary>
        public int IsDefault { get; set; } = 0;
    }
}
