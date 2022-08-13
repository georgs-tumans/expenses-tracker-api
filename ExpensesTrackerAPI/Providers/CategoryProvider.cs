using ExpensesTrackerAPI.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Providers
{
    /// <summary>
    /// Class for interaction with category data from the database
    /// </summary>
    public class CategoryProvider : ApiBaseProvider
    {
        public CategoryProvider(DbContext context) : base(context) { }

        /// <summary>
        /// Get a category entry from database
        /// </summary>
        /// <param name="categoryId">Category to get</param>
        /// <returns>A single category object or null</returns>
        public async Task<ExpenseCategory?> GetCategory(int categoryId)
        {
            return await _dbService.GetByCondition<ExpenseCategory>(x => x.CategoryId == categoryId && x.Active == 1).FirstOrDefaultAsync();
        }
    }
}
