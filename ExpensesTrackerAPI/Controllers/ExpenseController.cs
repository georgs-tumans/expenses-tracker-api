using Microsoft.AspNetCore.Mvc;

namespace ExpensesTrackerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExpenseController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ExpenseController> _logger;

        public ExpenseController(ILogger<ExpenseController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Expense> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Expense
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost]
        public void Add()
        {
            
        }
    }
}