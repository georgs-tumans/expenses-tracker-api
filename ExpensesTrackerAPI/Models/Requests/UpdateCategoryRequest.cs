using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Requests
{
    public class UpdateCategoryRequest
    {
        /// <summary>
        /// Id of the category
        /// </summary>
        [Required]
        public int Id { get; set; }
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
    }
}
