using ExpensesTrackerAPI.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Providers
{
    /// <summary>
    /// Provides database access for the weblog service
    /// </summary>
    public class WeblogProvider : ApiBaseProvider
    {
        public WeblogProvider(DbContext context) : base(context) { }

        /// <summary>
        /// Writes a log entry to the database
        /// </summary>
        /// <param name="entry">The log entry to write</param>
        /// <returns></returns>
        public void AddEntry(Weblog entry)
        {
            _dbService.Add<Weblog>(entry);
            _dbContext.SaveChanges();
        }
    }
}
