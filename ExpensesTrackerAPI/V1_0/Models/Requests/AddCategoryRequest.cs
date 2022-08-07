using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class AddCategoryRequest
    {
        /// <summary>
        /// Name of the category
        /// </summary>
        [Required]
        [StringLength(maximumLength: 50, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9](?:[a-zA-Z0-9.,'_ -]*[a-zA-Z0-9])?$", ErrorMessage = "Category name can only contain letters and numbers")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Category description
        /// </summary>
        [Required]
        [StringLength(maximumLength: 2000, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9](?:[a-zA-Z0-9.,'_ -]*[a-zA-Z0-9])?$", ErrorMessage = "Category description can only contain letters and numbers")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Every user has default pre-defined categories, that cannot be edited and are the same across all users. Only admin can
        /// create a default category
        /// </summary>
        public int? IsDefault { get; set; }
    }
}
