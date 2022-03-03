using ExpensesTrackerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpensesTrackerAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly ILogger<ExpenseController> _logger;
        private readonly ExpenseDbContext _dbContext;

        public ExpenseController(ILogger<ExpenseController> logger, ExpenseDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        [HttpPost]
        public void Add()
        {
            
        }

        [HttpGet]
        public async Task<ActionResult<List<Expense>>> Get()
        {
            return Ok(await _dbContext.Expenses.ToListAsync());
        }
    }
}