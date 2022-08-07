using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class AddExpenseRequest
    {
        //Amount and category id are nullable types because otherwise their value is automatically set to 0 when it's null in an incoming json.
        //To get the automatically generated error message, they must be nullable and have the 'Required' tag 

        /// <summary>
        /// The amount of the expense
        /// </summary>
        [Required]
        public double? Amount { get; set; }
        /// <summary>
        /// Some description of the expense
        /// </summary>
        [Required]
        public string Description { get; set; } = String.Empty;
        /// <summary>
        /// The category id the expense belongs to
        /// </summary>
        [Required]
        public int? CategoryId { get; set; }
    }
}
