namespace ExpensesTrackerAPI.Helpers
{
    public class LogException : Exception
    {
        public LogException (string msg, string st, string info)
        {
            Message = msg;
            StackTrace = st;
            Info = info;
        }

        public string Message { get; set; } = string.Empty;
        public string?  StackTrace { get; set; }
        public string? Info { get; set; }
    }
}
