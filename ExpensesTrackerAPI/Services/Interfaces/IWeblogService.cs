namespace ExpensesTrackerAPI.Helpers
{
    /// <summary>
    /// Provides logging to a database table
    /// </summary>
    public interface IWeblogService
    {
        /// <summary>
        /// Writes a message to a database table
        /// </summary>
        /// <param name="msg">The message to write</param>
        /// <param name="level">Level of the message - debug(1), info(2), warning(3), error(4) </param>
        /// <param name="stackTrace">Stacktrace for errors</param>
        /// <param name="info1">Additional information field 1</param>
        /// <param name="info2">Additional information field 2</param>
        /// <param name="userId">Current user ID</param>
        /// <returns></returns>
        public void LogMessage(string msg, int level, string? stackTrace = null, string? info1 = null, string? info2 = null, int? userId = null);
    }
}
