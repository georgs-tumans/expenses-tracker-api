using ExpensesTrackerAPI.Providers;

namespace ExpensesTrackerAPI.Helpers
{
    /// <summary>
    /// Various helper methods for controllers
    /// </summary>
    public class ControllerHelper
    {
        private readonly CategoryProvider _catProvider;
        private readonly UserProvider _userProvider;
        public ControllerHelper(ExpenseDbContext context)
        {
            _catProvider = new CategoryProvider(context);
            _userProvider = new UserProvider(context);
        }

        /// <summary>
        /// Validates whether a user has access to a particular category
        /// </summary>
        /// <param name="catId">The category to validate</param>
        /// <param name="userId">The user to validate</param>
        /// <param name="isAdmin">Whether the user is an admin</param>
        /// <returns>Whether the particular user has access to the particular category</returns>
        public async Task<bool> ValidateCategory(int catId, int userId, bool isAdmin)
        {
            bool found = false;

            if (catId == 0)
                return found;

            var cat = await _catProvider.GetCategory(catId);
    
            if (cat is not null)
            {
                //Check if this user has such a category (only for non-default ones). Admins can add expenses to any categories
                if (cat.IsDefault == 0 && !isAdmin)
                    found = _userProvider.CheckIfUserHasCategory(userId, catId);
                else found = true;
            }

            return found;
        }
    }
}
