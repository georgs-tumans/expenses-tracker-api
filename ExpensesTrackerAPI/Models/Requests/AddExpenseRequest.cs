using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class AddExpenseRequest
    {
        /// <summary>
        /// The amount of the expense
        /// </summary>
        //nullable type because otherwise this property is automatically set to 0 when it's null in an incoming json.
        //To get the automatically generated error message, this has to be nullable and have the 'Required' tag 
        [Required]
        public double? Amount { get; set; } 
        /// <summary>
        /// Some description of the expense
        /// </summary>
        public string Description { get; set; } = String.Empty;
    }
}
