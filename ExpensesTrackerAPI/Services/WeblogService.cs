using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Providers;

namespace ExpensesTrackerAPI.Helpers
{
    public class WeblogService : IWeblogService
    {
        private readonly ILogger<WeblogService> _logger;
        private readonly WeblogProvider _weblogProvider;
        public WeblogService(ExpenseDbContext context, ILogger<WeblogService> logger)
        {
            _logger = logger;
            _weblogProvider = new WeblogProvider(context);
        }

        
        public void LogMessage(string msg, int level, string? stackTrace = null, string? info1 = null, string? info2 = null, int? userId = null)
        {
            try
            {
                _weblogProvider.AddEntry(new Weblog
                {
                    LogMessage = msg,
                    LogLevel = level,
                    LogInfo1 = info1,
                    LogInfo2 = info2,
                    StackTrace = stackTrace,
                    LogTime = DateTime.UtcNow,
                    UserId = userId
                });
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
