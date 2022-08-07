using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpensesTrackerAPI.Models.Database
{
    public class Expense
    {
        [Required]
        [Key]
        public int ExpenseId { get; set; }
        [ForeignKey("CategoryId")]
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        [Comment("Textual description of the expense")]
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        [Required]
        [ForeignKey("UserId")]
        public int UserId { get; set; }
    }
}