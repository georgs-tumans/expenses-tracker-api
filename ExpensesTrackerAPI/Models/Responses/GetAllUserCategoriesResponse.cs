namespace ExpensesTrackerAPI.Models.Responses
{
    public class GetAllUserCategoriesResponse
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Pre-made categories that are same for every user and can only be edited by admins
        /// </summary>
        public int Default { get; set; }
    }
}
