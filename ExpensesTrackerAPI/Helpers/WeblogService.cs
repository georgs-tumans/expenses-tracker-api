using ExpensesTrackerAPI.Models.Database;

namespace ExpensesTrackerAPI.Helpers
{
    public class WeblogService : IWeblogService
    {
        private readonly ExpenseDbContext _dbContext;
        private readonly ILogger<WeblogService> _logger;
        public WeblogService(ExpenseDbContext context, ILogger<WeblogService> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public void LogMessage(string msg, int level, string? stackTrace = null, string? info1 = null, string? info2 = null)
        {
            try
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
            catch (Exception ex)
            {
                //At first we log the reason that the proper logging to DB failed
                _logger.LogError(ex.ToString());

                //Then we log the actual message to event source (should be trace logs on windows)
                LogException exception = new LogException(msg, stackTrace, $"{info1}; {info2}");
                switch (level)
                {
                    case (int)LogLevel.Error:
                        _logger.LogError(exception.ToString());
                        break;
                    case (int)LogLevel.Warning:
                        _logger.LogWarning(exception.ToString());
                        break;
                    case (int)LogLevel.Information:
                        _logger.LogInformation(exception.ToString()); 
                        break;
                    default:
                        _logger.LogDebug(exception.ToString());
                        break;
                }
            }
            
        }
    }
}
