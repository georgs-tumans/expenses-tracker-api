using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class UpdateExpenseRequest
    {
        //Id and Amount are nullable types because otherwise these properties are automatically set to 0 when being null in an incoming json.
        //To get the automatically generated error message, they must be nullable and have the 'Required' tag 

        [Required]
        public int? Id { get; set; }
        /// <summary>
        /// The amount of the expense
        /// </summary>
        [Required]
        public double? Amount { get; set; }
        /// <summary>
        /// Some description of the expense
        /// </summary>
        public string Description { get; set; } = String.Empty;
        /// <summary>
        /// User who is trying to make the update
        /// </summary>
        [Required]
        public int UserId { get; set; }
    }
}
