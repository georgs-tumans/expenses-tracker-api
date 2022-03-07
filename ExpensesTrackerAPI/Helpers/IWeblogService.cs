namespace ExpensesTrackerAPI.Helpers
{
    public interface IWeblogService
    {
        public void LogMessage(string msg, int level, string? stackTrace = null, string? info1 = null, string? info2 = null);
    }
}
