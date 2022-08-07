using ExpensesTrackerAPI.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Providers
{
    /// <summary>
    /// A data provider for user related queries
    /// </summary>
    public class UserProvider : ApiBaseProvider
    {
        public UserProvider(DbContext context) : base(context) { }
        
#region GetMethods
        /// <summary>
        /// Returns a list of all existing users from database
        /// </summary>
        /// <param name="onlyActive">1 - get only active users; 0 - get all users</param>
        /// <returns></returns>
        public async Task<List<User>> GetAllUsersAsync(int? onlyActive = 1)
        {
            if (onlyActive > 0)
                return await _dbService.GetByCondition<User>(x => x.Active == 1).ToListAsync();
            else
                return await _dbService.GetAll<User>().OrderBy(x => x.Username).ToListAsync();
        }

        /// <summary>
        /// Returns full data on a single user (either active or inactive)
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>A single user object or null</returns>
        public async Task<User> GetUserAsync(int userId)
        {
            return await _dbService.GetByCondition<User>(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns full data on a single active user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>A single user object or null</returns>
        public async Task<User> GetOnlyActiveUserAsync(int userId)
        {
            return await _dbService.GetByCondition<User>(x => x.UserId == userId && x.Active == 1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a user entry by the provided email
        /// </summary>
        /// <param name="userId">email of the user</param>
        /// <returns></returns>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _dbService.GetByCondition<User>(x => x.Email == email).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a user entry by the provided username
        /// </summary>
        /// <param name="userId">username of the user</param>
        /// <returns></returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _dbService.GetByCondition<User>(x => x.Username == username).FirstOrDefaultAsync();
        }


#endregion GetMethods

        /// <summary>
        /// Updates a user entry in database
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns></returns>
        public async Task UpdateUserAsync(User user)
        {
            _dbService.Update<User>(user);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Checks whether a particular user is an administrator
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <returns>Whether the user is an administrator</returns>
        public bool CheckIfUserIsAdmin(int userId)
        {
            return _dbService.GetByCondition<User>(x => x.UserId == userId && x.AccountType == (int)UserType.Administrator).Any();
        }
    }
}
