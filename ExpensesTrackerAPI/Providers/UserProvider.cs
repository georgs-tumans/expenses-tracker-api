using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

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
        /// <returns>A list of user obects</returns>
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
        public async Task<User?> GetUserAsync(int userId)
        {
            return await _dbService.GetByCondition<User>(x => x.UserId == userId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Returns full data on a single active user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <returns>A single user object or null</returns>
        public async Task<User?> GetOnlyActiveUserAsync(int userId)
        {
            return await _dbService.GetByCondition<User>(x => x.UserId == userId && x.Active == 1).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a user entry by the provided email
        /// </summary>
        /// <param name="userId">email of the user</param>
        /// <returns>A single user object or null</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbService.GetByCondition<User>(x => x.Email == email).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a user entry by either email or username
        /// </summary>
        /// <param name="authString">A string containing either an email address or username</param>
        /// <returns>A single user object or null</returns>
        public async Task<User?> GetUserByAuthStringAsync(string authString)
        {
            var user = await GetUserByUsernameAsync(authString);
            if (user == null)
                user = await GetUserByEmailAsync(authString);

            return user;
        }

        /// <summary>
        /// Gets a user entry by the provided username
        /// </summary>
        /// <param name="userId">username of the user</param>
        /// <returns>A single user object or null</returns>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbService.GetByCondition<User>(x => x.Username == username).FirstOrDefaultAsync();
        }


        #endregion GetMethods

        /// <summary>
        /// Updates a user entry in database
        /// </summary>
        /// <param name="request">Incoming user update request object</param>
        /// <param name="userId">ID of the currently logged-in user</param>
        /// <returns>The updated user object</returns>
        public async Task<User> UpdateUserAsync(UpdateUserRequest request, int userId)
        {
            var user = await GetOnlyActiveUserAsync(userId);

            if (user is null)
                throw new ArgumentNullException("User not found");

            if (!String.IsNullOrEmpty(request.Email))
            {
                if (!MailAddress.TryCreate(request.Email, out var email))
                    throw new ArgumentException("Invalid email provided");

                //Check if email is not taken by another user already
                var userByEmail = await GetUserByEmailAsync(request.Email);
                if (userByEmail is not null && userByEmail.UserId != userId)
                    throw new ArgumentException("E-mail already taken");

                user.Email = request.Email;
            }

            if (!String.IsNullOrEmpty(request.Username))
            {
                //Check if username is not taken by another user already
                var userByUsername = await GetUserByUsernameAsync(request.Username);
                if (userByUsername is not null && userByUsername.UserId != userId)
                    throw new ArgumentException("Username already taken");

                user.Username = request.Username;
            }

            user.Surname = String.IsNullOrEmpty(request.Surname) ? user.Surname : request.Surname;
            user.Name = String.IsNullOrEmpty(request.Name) ? user.Name : request.Name;
            user.PhoneNumber = String.IsNullOrEmpty(request.PhoneNumber) ? user.PhoneNumber : request.PhoneNumber;

            _dbService.Update<User>(user);
            await _dbContext.SaveChangesAsync();
            return user;
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

        /// <summary>
        /// Checks whether a particular user has a particular category
        /// </summary>
        /// <param name="userId">ID of the user to check</param>
        /// <param name="catId">ID of the category to check</param>
        /// <returns>Whether the user has the category or not</returns>
        public bool CheckIfUserHasCategory(int userId, int catId)
        {
            return _dbService.GetByCondition<UserToCategory>(x => x.UserId == userId && x.CategoryId == catId).Any();
        }

        /// <summary>
        /// Deletes (de-activates) a user entry in database
        /// </summary>
        /// <param name="userToDelete">ID of the user to delete (de-activate)</param>
        /// <returns></returns>
        public async Task DeactivateUser(int userToDelete)
        {
            var user = await GetOnlyActiveUserAsync(userToDelete);

            if (user is null)
                throw new ArgumentNullException("User not found");

            user.Active = 0;
            _dbService.Update(user);

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Activates a user account in database
        /// </summary>
        /// <param name="user">User to activate</param>
        /// <returns></returns>
        public async Task ActivateUserAsync(User user)
        {
            user.Active = 1;
            _dbService.Update<User>(user);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Changes the account type of a user
        /// </summary>
        /// <param name="userId">ID of the user to change the account type for</param>
        /// <param name="userId">The type to be changed to</param>
        /// <returns></returns>
        public async Task ChangeUserAccountType(int userId, UserType type)
        {
            var user = await GetOnlyActiveUserAsync(userId);

            if (user is null)
                throw new ArgumentNullException("User not found");

            user.AccountType = (int)type;
            _dbService.Update(user);

            await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Creates a new user entry in database and sends a verification email
        /// </summary>
        /// <param name="request">The user register request</param>
        /// <param name="userId">Authentification service</param>
        /// <returns>The newly created user object</returns>
        public async Task<User> RegisterNewUserAsync(RegisterUserRequest request, IAuthService authService, LinkGenerator linkGenerator, HttpContext context)
        {
            authService.HashPassword(request.Password.Trim(), out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User()
            {
                Name = request.Name != null ? request.Name.Trim() : null,
                Surname = request.Surname != null ? request.Surname.Trim() : null,
                Email = request.Email.Trim(),
                PhoneNumber = request.PhoneNumber != null ? request.PhoneNumber.Trim() : null,
                Username = request.UserName.Trim(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Active = 0,
                AccountType = (int)UserType.User,
                RegistrationDate = DateTime.UtcNow
            };
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction())
            {
                _dbService.Add<User>(user);
                await _dbContext.SaveChangesAsync();

                string ctoken = Guid.NewGuid().ToString();
                user.EmailConfirmationToken = ctoken;
                user.EmailConfirmationTokenRegistration = DateTime.UtcNow;

                string? link = linkGenerator.GetUriByAction(context, "ConfirmEmail", "Auth", new { id = user.UserId, token = ctoken });
                if (!String.IsNullOrEmpty(link))
                {
                    authService.SendConfirmationEmail(ctoken, link, user);
                    await dbContextTransaction.CommitAsync();
                }
                    
                else
                {
                    await dbContextTransaction.RollbackAsync();
                    throw new ArgumentNullException(link);
                }
                    
            }
            return user;
        }
    }
}
