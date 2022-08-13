namespace ExpensesTrackerAPI.Extensions
{
    /// <summary>
    /// Datetime class extensions
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Sets a datetime to UTC format
        /// </summary>
        /// <param name="dateTime">The date to format</param>
        /// <returns>A UTC formatted datetime</returns>
        public static DateTime? SetKindUtc(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                return dateTime.Value.SetKindUtc();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets a datetime to UTC format
        /// </summary>
        /// <param name="dateTime">The date to format</param>
        /// <returns>A UTC formatted datetime</returns>
        public static DateTime SetKindUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;

            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
    }
}
