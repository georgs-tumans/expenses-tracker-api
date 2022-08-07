using ExpensesTrackerAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Providers
{
    /// <summary>
    /// A base class for API data providers containing common fields and functionality
    /// </summary>
    public abstract class ApiBaseProvider
    {
        protected readonly DbContext _dbContext;
        protected readonly IDbService _dbService;

        public ApiBaseProvider(DbContext context)
        {
            _dbContext = context;
            _dbService = new DbService(_dbContext);
        }
    }
}
