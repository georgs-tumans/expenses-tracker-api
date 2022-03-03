namespace ExpensesTrackerAPI.Models.Requests
{
    public class AddExpenseRequest
    {
        /// <summary>
        /// The amount of the expense
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Some description of the expense
        /// </summary>
        public string Description { get; set; }    
    }
}
