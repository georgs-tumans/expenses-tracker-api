using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace ExpensesTrackerAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ExpenseController : ControllerBase
    {
        private readonly IWeblogService _logger;
        private readonly ExpenseDbContext _dbContext;

        public ExpenseController(IWeblogService logger, ExpenseDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        [HttpPost]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult<AddExpenseResponse>> Add(AddExpenseRequest expense)
        {
            try
            {
                var newExpense = new Expense
                {
                    Amount = (double) expense.Amount, //won't be null because of automatic incoming object validation
                    Note = expense.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Expenses.Add(newExpense);
                await _dbContext.SaveChangesAsync();

                return Ok(new AddExpenseResponse
                {
                    ExpenseId = newExpense.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"Error executing ExceptionController.Add: {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult> Update(UpdateExpenseRequest requestExpense)
        {
            try
            { 
                var dbExpense = await _dbContext.Expenses.Where(x => x.Id == requestExpense.Id).FirstAsync();
                if (dbExpense == null)
                {
                    _logger.LogMessage("[ExpenseController.Update] Expense not found", (int)Helpers.LogLevel.Information, null, $"id: {requestExpense.Id}");
                    return NotFound("Expense not found");
                } 
                else
                {
                    dbExpense.Amount = (int)requestExpense.Amount; //won't be null because of automatic incoming object validation
                    dbExpense.Note = requestExpense.Description;
                    _dbContext.Expenses.Update(dbExpense);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult> Delete([Required]int id)
        {
            try
            {
                var dbExpense = await _dbContext.Expenses.FindAsync(id);
                if (dbExpense == null)
                {
                    _logger.LogMessage("[ExpenseController.Delete] Expense not found", (int)Helpers.LogLevel.Information, null, $"id: {id}");
                    return NotFound("Expense not found");
                } 
                else
                {
                    _dbContext.Expenses.Remove(dbExpense);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(500, Description = "Internal server error")]
        public async Task<ActionResult<List<Expense>>> GetAll()
        {
            try
            {
                return Ok(await _dbContext.Expenses.OrderBy(x => x.Id).ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAll] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(500, Description = "Internal server error")]
        [SwaggerResponse(404, Description = "Not found")]
        public async Task<ActionResult<Expense>> Get([Required]int id)
        {
            try
            {
                var expense = await _dbContext.Expenses.FindAsync(id);
                if (expense == null)
                {
                    _logger.LogMessage("[ExpenseController.Get] Expense not found", (int)Helpers.LogLevel.Information, null, $"id: {id}");
                    return NotFound("Expense not found");
                }
                else
                    return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Get] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }
    }
}