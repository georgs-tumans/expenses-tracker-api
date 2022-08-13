using ExpensesTrackerAPI.Extensions;
using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Providers
{
    /// <summary>
    /// Class that provides expense data from the database
    /// </summary>
    public class ExpenseProvider : ApiBaseProvider
    {
        public ExpenseProvider(DbContext context) : base(context) { }

#region GetMethods
        /// <summary>
        /// Adds a new expense item to the database
        /// </summary>
        /// <param name="expense">The expense object</param>
        /// <returns>A single expense object or null</returns>
        public async Task<Expense?> GetUserExpense(int expenseId, int userId, bool isAdmin)
        {
            //Admin users have access to all expenses
            return await _dbService.GetByCondition<Expense>(x => x.ExpenseId == expenseId && (x.UserId == userId || isAdmin)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns a list of all user expenses
        /// </summary>
        /// <param name="userId">The currently logged-in user</param>
        /// <param name="dateFrom">Expense date from filter</param>
        /// <param name="dateTo">Expense date to filter</param>
        /// <param name="category">Expense category filter</param>
        /// <returns>A list of expenses</returns>
        public async Task<List<Expense>> GetAllUserExpenses(int userId, DateTime? dateFrom, DateTime? dateTo, int? category)
        {
            
            var resultList = _dbService.GetByCondition<Expense>(x => x.UserId == userId).OrderBy(x => x.ExpenseId);

            var filteredList = FilterExpenseList(resultList, dateFrom, dateTo, category);

            return await filteredList.ToListAsync();
        }

        /// <summary>
        /// Returns a list of all expenses from all users
        /// </summary>
        /// <param name="dateFrom">Expense date from filter</param>
        /// <param name="dateTo">Expense date to filter</param>
        /// <param name="category">Expense category filter</param>
        /// <returns>A list of expenses</returns>
        public async Task<List<Expense>> GetAllExpenses(DateTime? dateFrom, DateTime? dateTo, int? category)
        {

            var resultList = _dbService.GetAll<Expense>().OrderBy(x => x.ExpenseId);

            var filteredList = FilterExpenseList(resultList, dateFrom, dateTo, category);

            return await filteredList.ToListAsync();
        }

#endregion GetMethods

        /// <summary>
        /// Adds a new expense item to the database
        /// </summary>
        /// <param name="request">The incoming new expense request object</param>
        /// <param name="userId">The currently logged-in user id</param>
        /// <returns>ID of the newly created database entry</returns>
        public async Task<int> AddNewExpense(AddExpenseRequest request, int userId)
        {
            var newExpense = new Expense
            {
                Amount = (double)request.Amount, //won't be null because of automatic incoming object validation
                Note = request.Description,
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                CategoryId = (int)request.CategoryId //won't be null because of automatic incoming object validation
            };

            _dbService.Add<Expense>(newExpense);
            await _dbContext.SaveChangesAsync();
            return newExpense.ExpenseId;
        }

        /// <summary>
        /// Updates an existing database expense entry
        /// </summary>
        /// <param name="expense">The incoming update request object</param>
        /// <param name="userId">Currently logged in user</param>
        /// <param name="isAdmin">Whether the user is an admin</param>
        /// <returns>The updated expense object</returns>
        public async Task<Expense> UpdateExpense(UpdateExpenseRequest request, int userId, bool isAdmin)
        {
            var userEpense = await GetUserExpense((int)request.Id, userId, isAdmin);

            if (userEpense is null)
                throw new ArgumentNullException("Expense not found");

            userEpense.Amount = request.Amount is null || request.Amount == 0 ? userEpense.Amount : (int)request.Amount;
            userEpense.Note = String.IsNullOrEmpty(request.Description) ? userEpense.Note : request.Description;
            userEpense.CategoryId = (int)request.CategoryId; 

            _dbService.Update<Expense>(userEpense);
            await _dbContext.SaveChangesAsync();
            return userEpense;
        }

        /// <summary>
        /// Updates an existing database expense entry
        /// </summary>
        /// <param name="expenseId">ID of the expense to delete</param>
        /// <param name="userId">Currently logged in user</param>
        /// <param name="isAdmin">Whether the user is an admin</param>
        /// <returns></returns>
        public async Task DeleteExpense(int expenseId, int userId, bool isAdmin)
        {
            var userEpense = await GetUserExpense(expenseId, userId, isAdmin);

            if (userEpense is null)
                throw new ArgumentNullException("Expense not found");

            _dbService.Delete<Expense>(userEpense);
            await _dbContext.SaveChangesAsync();
        }

        private IQueryable<Expense> FilterExpenseList(IQueryable<Expense> expenseList, DateTime? dateFrom, DateTime? dateTo, int? category)
        {
            
            if (dateFrom != null)
                expenseList = (IOrderedQueryable<Expense>)expenseList.Where(x => x.CreatedAt >= dateFrom.SetKindUtc());

            if (dateTo != null)
                expenseList = (IOrderedQueryable<Expense>)expenseList.Where(x => x.CreatedAt <= dateTo.SetKindUtc());

            if (category != null)
                expenseList = (IOrderedQueryable<Expense>)expenseList.Where(x => x.CategoryId == category);

            return expenseList;
        }
    }
}
