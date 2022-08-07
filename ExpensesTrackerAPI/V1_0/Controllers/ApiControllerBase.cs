using ExpensesTrackerAPI.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpensesTrackerAPI.V1_0.Controllers
{
    /// <summary>
    /// A base class for an API controller that contains common controller functionality.
    /// </summary>
    public abstract class ApiControllerBase : ControllerBase
    {
        private readonly UserProvider _userProvider;
        protected readonly DbContext _dbContext;
        /// <summary>
        /// ID of the currently logged-in user
        /// </summary>
        protected int UserId { get => GetUserId(); }
        /// <summary>
        /// Checks whether the currently logged-in user is an administrator
        /// </summary>
        protected bool IsAdmin { get => GetIsAdmin(); }

        public ApiControllerBase(DbContext context)
        {
            _dbContext = context;
            _userProvider = new UserProvider(context);
        }

        /// <summary>
        /// Checks whether the current user is an admin level user
        /// </summary>
        private bool GetIsAdmin()
        {
            return _userProvider.CheckIfUserIsAdmin(UserId);
        }

        /// <summary>
        /// Gets the currently logged in user ID
        /// </summary>
        private int GetUserId()
        {
            try
            {
                return int.Parse(User.Claims.First(x => x.Type == ClaimTypes.PrimarySid).Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get user id from the auth token: " + ex.Message);
            }
        }
    }
}
