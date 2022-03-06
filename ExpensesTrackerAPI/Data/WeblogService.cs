using ExpensesTrackerAPI.Models.Database;

namespace ExpensesTrackerAPI.Data
{
    public class WeblogService
    {
        private readonly ExpenseDbContext _dbContext;
        public WeblogService(ExpenseDbContext context)
        {
            _dbContext = context;
        }

        public void LogMessage(string msg, int level, string? stackTrace = null, string? info1 = null, string? info2 = null)
        {
            _dbContext.Weblogs.Add(new Weblog
            {
              LogMessage = msg,
              LogLevel = level,
              LogInfo1 = info1,
              LogInfo2 = info2,
              StackTrace = stackTrace,
              LogTime = DateTime.UtcNow
            });

            _dbContext.SaveChanges();
        }
    }
}
