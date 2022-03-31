using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Models.Database
{
    public class Weblog
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public DateTime LogTime { get; set; }
        [Required]
        public int LogLevel { get; set; }
        [Required]
        public string LogMessage {get; set;}
        public string? LogInfo1 { get; set;}
        public string? LogInfo2 { get; set;} 
        public string? StackTrace { get; set;}
        public int? UserId { get; set; }

    }
}
