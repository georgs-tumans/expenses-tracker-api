using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
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
        public async Task<ExpenseCategory?> GetCategoryAsync(int categoryId)
        {
            return await _dbService.GetByCondition<ExpenseCategory>(x => x.CategoryId == categoryId && x.Active == 1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all active categories
        /// </summary>
        /// <returns>List of all actvie categories</returns>
        public async Task<List<GetAllUserCategoriesResponse>> GetAllActiveCategoriesAsync()
        {
            return await _dbService.GetByCondition<ExpenseCategory>(x => x.Active == 1).Select(cat => new GetAllUserCategoriesResponse
            {
                CategoryId = cat.CategoryId,
                Name = cat.Name,
                Description = cat.Description,
                Default = cat.IsDefault
            }).ToListAsync();
        }

        /// <summary>
        /// Gets all categories of a particular user (inlcuding the default ones)
        /// </summary>
        /// <param name="userId">User who's categories to return</param>
        /// <returns>A list of all user categories including the default ones</returns>
        public async Task<List<GetAllUserCategoriesResponse>> GetAllUserCategoriesAsync(int userId)
        {
            var defaultCats = _dbService.GetByCondition<ExpenseCategory>(x => x.Active == 1 && x.IsDefault == 1);
            var userCats = _dbService.GetByJoin<ExpenseCategory, UserToCategory, GetAllUserCategoriesResponse>(c => c.Active == 1,
                                                                                                             u => u.UserId == userId,
                                                                                                             c => c.CategoryId,
                                                                                                             u => u.CategoryId,
                                                                                                             (c, u) => new GetAllUserCategoriesResponse()
                                                                                                             {
                                                                                                                 CategoryId = c.CategoryId,
                                                                                                                 Name = c.Name,
                                                                                                                 Description = c.Description,
                                                                                                                 Default = c.IsDefault
                                                                                                             }).Union(defaultCats.Select(cat => new GetAllUserCategoriesResponse
                                                                                                             {
                                                                                                                 CategoryId = cat.CategoryId,
                                                                                                                 Name = cat.Name,
                                                                                                                 Description = cat.Description,
                                                                                                                 Default = cat.IsDefault
                                                                                                             }));

            return await userCats.ToListAsync();
        }

        /// <summary>
        /// Adds a new category to the database
        /// </summary>
        /// <param name="request">Incoming add category request</param>
        /// <param name="userId">Currently logged-in user id</param>
        /// <returns>ID of the newly added categoty entry</returns>
        public async Task<int> AddNewCategoryAsync(AddCategoryRequest request, int userId)
        {

            var newCat = new ExpenseCategory
            {
                Name = request.Name,
                Description = request.Description,
                Active = 1,
                IsDefault = request.IsDefault is not null ? (int)request.IsDefault : 0
            };

            _dbService.Add<ExpenseCategory>(newCat);
            await _dbContext.SaveChangesAsync();

            //non-default categories are tied to individual users who create them
            if (newCat.IsDefault == 0)
            {
                _dbService.Add<UserToCategory>(new UserToCategory
                {
                    CategoryId = newCat.CategoryId,
                    UserId = userId
                });
                await _dbContext.SaveChangesAsync();
            }

            return newCat.CategoryId;
        }

        /// <summary>
        /// Updates an existing category entry in database
        /// </summary>
        /// <param name="request">Incoming update category request</param>
        /// <param name="category">The category object to update</param>
        /// <returns></returns>
        public async Task UpdateCategoryAsync(UpdateCategoryRequest request, ExpenseCategory category)
        {
            category.Description = String.IsNullOrEmpty(request.Description) ? category.Description : request.Description;
            category.Name = String.IsNullOrEmpty(request.Name) ? category.Name : request.Name;

            _dbService.Update<ExpenseCategory>(category);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete (de-activate) a category in database
        /// </summary>
        /// <param name="category">The category object to delete (de-activate)</param>
        /// <returns></returns>
        public async Task DeleteCategoryAsync(ExpenseCategory category)
        {
            category.Active = 0;
            _dbService.Update<ExpenseCategory>(category);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Check whether a particular user has a particular category
        /// </summary>
        /// <param name="categoryId">ID of the category</param>
        /// <param name="userId">ID of the user</param>
        /// <returns>Whether the user has the category</returns>
        public async Task<bool> CheckIfUserHasCategoryAsync(int categoryId, int userId)
        {
            return await _dbService.GetByCondition<UserToCategory>(x => x.CategoryId == categoryId && x.UserId == userId).AnyAsync();
        }
    }
}
