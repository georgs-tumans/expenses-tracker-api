using ExpensesTrackerAPI.Models;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace ExpensesTrackerAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
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
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult<AddExpenseResponse>> Add(AddExpenseRequest expense)
        {
            try
            {
                var newExpense = new Expense
                {
                    Amount = expense.Amount,
                    Note = expense.Description
                };
                _dbContext.Expenses.Add(newExpense);
                await _dbContext.SaveChangesAsync();
                return new AddExpenseResponse
                {
                    ExpenseId = newExpense.Id
                };
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult<List<Expense>>> Get()
        {
            try
            {
                return Ok(await _dbContext.Expenses.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            
        }
    }
}