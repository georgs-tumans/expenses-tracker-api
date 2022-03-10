using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace ExpensesTrackerAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    [SwaggerResponse(401, Description = "Unauthorized")]
    [SwaggerResponse(500, Description = "Internal server error")]
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
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        
        public async Task<ActionResult<AddExpenseResponse>> Add(AddExpenseRequest request)
        {
            try
            {
                var newExpense = new Expense
                {
                    Amount = (double)request.Amount, //won't be null because of automatic incoming object validation
                    Note = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UserId = request.UserId,
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
                _logger.LogMessage($"[ExpenseController.Add] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        public async Task<ActionResult> Update(UpdateExpenseRequest request)
        {
            try
            { 
                //Admin users should be able to update any expense
                bool isAdmin = await _dbContext.Users.Where(x => x.Id == request.UserId && x.AccountType == (int)UserType.Administrator).AnyAsync();
                var dbExpense = await _dbContext.Expenses.Where(x => x.Id == request.Id && (request.UserId == x.UserId || isAdmin)).FirstAsync();
                
                if (dbExpense == null)
                {
                    _logger.LogMessage("[ExpenseController.Update] Expense not found", (int)Helpers.LogLevel.Information, null, $"id: {request.Id}");
                    return NotFound("Expense not found or you have no access to it");
                } 
                else
                {
                    dbExpense.Amount = (int)request.Amount; //won't be null because of automatic incoming object validation
                    dbExpense.Note = request.Description;
                    _dbContext.Expenses.Update(dbExpense);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpDelete]
        [Route("api/v{version:apiVersion}/[controller]")]
        [Authorize(Roles = "user,admin")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        public async Task<ActionResult> Delete([Required] int expenseId, [Required] int userId)
        {
            try
            {
                //Admin users should be able to delete any expense
                bool isAdmin = await _dbContext.Users.Where(x => x.Id == userId && x.AccountType == (int)UserType.Administrator).AnyAsync();
                var dbExpense = await _dbContext.Expenses.Where(x => x.Id == expenseId && (x.UserId == userId || isAdmin)).FirstAsync();

                if (dbExpense == null)
                {
                    _logger.LogMessage("[ExpenseController.Delete] Expense not found", (int)Helpers.LogLevel.Information, null, $"expenseId: {expenseId}, userId: {userId}");
                    return NotFound("Expense not found or you have no access to it");
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
                _logger.LogMessage($"[ExpenseController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"expenseId: {expenseId}, userId: {userId}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetAllUser")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<List<Expense>>> GetAll([Required] int userId, DateTime? dateFrom, DateTime? dateTo, int? category, int? limit)
        {
            try
            {
                var resultList = _dbContext.Expenses.Where(x => x.UserId == userId).OrderBy(x => x.Id);

                if (dateFrom != null)
                    resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt >= dateFrom);

                if (dateTo != null)
                    resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt <= dateTo);

                if (limit != null)
                    resultList = (IOrderedQueryable<Expense>)resultList.Take((int)limit);

                return Ok(await resultList.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAll] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"dateFrom: {dateFrom}, dateTo: {dateTo}, userId: {userId}, category: {category}, limit: {limit}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetAllAdmin")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.Forbidden)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(403, Description = "Forbidden")]
        public async Task<ActionResult<List<Expense>>> GetAllAdmin([Required] int currentUserId, DateTime? dateFrom, DateTime? dateTo, int? category, int? limit, int? filterUserId)
        {
            try
            {
                bool isAdmin = await _dbContext.Users.Where(x => x.Id == currentUserId && x.AccountType == (int)UserType.Administrator).AnyAsync();

                if (isAdmin)
                {
                    var resultList = _dbContext.Expenses.OrderBy(x => x.Id);

                    if (filterUserId != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.UserId == filterUserId);

                    if (dateFrom != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt >= dateFrom);

                    if (dateTo != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt <= dateTo);

                    if (limit != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Take((int)limit);

                    return Ok(await resultList.ToListAsync());
                }

                else return StatusCode((int)HttpStatusCode.Forbidden);


            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAllAdmin] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}, limit: {limit}, filterUserId: { filterUserId}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

        [HttpGet]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetSingle/{expenseId}")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        public async Task<ActionResult<Expense>> Get([Required] int expenseId, [Required] int userId)
        {
            try
            {
                //Admin users should be able to get any expense
                bool isAdmin = await _dbContext.Users.Where(x => x.Id == userId && x.AccountType == (int)UserType.Administrator).AnyAsync();
                var expense = await _dbContext.Expenses.Where(x => x.Id == expenseId && (x.UserId == userId || isAdmin)).FirstAsync();

                if (expense == null)
                {
                    _logger.LogMessage("[ExpenseController.Get] Expense not found", (int)Helpers.LogLevel.Information, null, $"userId: {userId}, epenseId: {expenseId}");
                    return NotFound("Expense not found");
                }
                else
                    return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Get] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {userId}, epenseId: {expenseId}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }
    }
}