using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
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
        private int _userId;

        public ExpenseController(IWeblogService logger, ExpenseDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        [HttpPost]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.BadRequest)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(400, Description = "Bad request")]
        public async Task<ActionResult<AddExpenseResponse>> Add(AddExpenseRequest request)
        {
            try
            {
                _userId = GetUserId();

                if (await ValidateCategory((int)request.CategoryId) == false)
                    return BadRequest("Such expense category does not exist");

                var newExpense = new Expense
                {
                    Amount = (double)request.Amount, //won't be null because of automatic incoming object validation
                    Note = request.Description,
                    CreatedAt = DateTime.UtcNow,
                    UserId = _userId,
                    CategoryId = (int)request.CategoryId //won't be null because of automatic incoming object validation
                };

                _dbContext.Expenses.Add(newExpense);
                await _dbContext.SaveChangesAsync();

                return Ok(new AddExpenseResponse
                {
                    ExpenseId = newExpense.ExpenseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Add] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), $"Current user id: {_userId}");
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
                _userId = GetUserId();
                //Admin users should be able to update any expense
                bool isAdmin = await IsAdmin();
                var dbExpense = await _dbContext.Expenses.Where(x => x.ExpenseId == request.Id && (_userId == x.UserId || isAdmin)).FirstOrDefaultAsync();

                if (dbExpense == null)
                {
                    _logger.LogMessage("[ExpenseController.Update] Expense not found", (int)Helpers.LogLevel.Information, null, $"id: {request.Id}, userId: {_userId}");
                    return NotFound("Expense not found or you have no access to it");
                } 
                else
                {
                    if (await ValidateCategory((int)request.CategoryId) == false)
                        return BadRequest("Such expense category does not exist");

                    dbExpense.Amount = (int)request.Amount; //won't be null because of automatic incoming object validation
                    dbExpense.Note = request.Description;
                    dbExpense.CategoryId = (int)request.CategoryId;
                    _dbContext.Expenses.Update(dbExpense);
                    await _dbContext.SaveChangesAsync();
                    return Ok();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), $"Current user id: {_userId}");
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
        public async Task<ActionResult> Delete([Required] int expenseId)
        {
            try
            {
                _userId = GetUserId();
                //Admin users should be able to delete any expense
                bool isAdmin = await IsAdmin();
                var dbExpense = await _dbContext.Expenses.Where(x => x.ExpenseId == expenseId && (x.UserId == _userId || isAdmin)).FirstOrDefaultAsync();

                if (dbExpense == null)
                {
                    _logger.LogMessage("[ExpenseController.Delete] Expense not found", (int)Helpers.LogLevel.Information, null, $"expenseId: {expenseId}, userId: {_userId}");
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
                _logger.LogMessage($"[ExpenseController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"expenseId: {expenseId}, userId: {_userId}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetAllUser")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<List<Expense>>> GetAll(DateTime? dateFrom, DateTime? dateTo, int? category)
        {
            try
            {
                _userId = GetUserId();
                var resultList = _dbContext.Expenses.Where(x => x.UserId == _userId).OrderBy(x => x.ExpenseId);

                if (dateFrom != null)
                    resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt >= dateFrom);

                if (dateTo != null)
                    resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt <= dateTo);

                if (category != null)
                    resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CategoryId == category);


                return Ok(await resultList.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAll] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"dateFrom: {dateFrom}, dateTo: {dateTo}, userId: {_userId}, category: {category}");
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
        public async Task<ActionResult<List<Expense>>> GetAllAdmin(DateTime? dateFrom, DateTime? dateTo, int? category, int? filterUserId)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();

                if (isAdmin)
                {
                    var resultList = _dbContext.Expenses.OrderBy(x => x.ExpenseId);

                    if (filterUserId != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.UserId == filterUserId);

                    if (dateFrom != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt >= dateFrom);

                    if (dateTo != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CreatedAt <= dateTo);

                    if (category != null)
                        resultList = (IOrderedQueryable<Expense>)resultList.Where(x => x.CategoryId == category);

                  
                    return Ok(await resultList.ToListAsync());
                }

                else return StatusCode((int)HttpStatusCode.Forbidden);


            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAllAdmin] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}, filterUserId: { filterUserId}, current userId: {_userId}");
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
        public async Task<ActionResult<Expense>> Get([Required] int expenseId)
        {
            try
            {
                _userId = GetUserId();
                //Admin users should be able to get any expense
                bool isAdmin = await IsAdmin();
                var expense = await _dbContext.Expenses.Where(x => x.ExpenseId == expenseId && (x.UserId == _userId || isAdmin)).FirstOrDefaultAsync();

                if (expense == null)
                {
                    _logger.LogMessage("[ExpenseController.Get] Expense not found", (int)Helpers.LogLevel.Information, null, $"userId: {_userId}, epenseId: {expenseId}");
                    return NotFound("Expense not found or you have no access to it");
                }
                else
                    return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Get] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {_userId}, epenseId: {expenseId}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

        }

        private async Task<bool> IsAdmin()
        {
            return await _dbContext.Users.Where(x => x.UserId == _userId && x.AccountType == (int)UserType.Administrator).AnyAsync();
        }
        private int GetUserId()
        {
            try
            {
                return int.Parse(User.Claims.First(x => x.Type == ClaimTypes.PrimarySid).Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get user id from the auth token: " + ex.Message);
            }
            
        }

        private async Task<bool> ValidateCategory(int catId)
        {
            bool isAdmin = await IsAdmin();
            bool found = false;

            var cat = await _dbContext.ExpensesCategories.Where(x => x.CategoryId == catId && x.Active == 1).FirstOrDefaultAsync();

            if (cat is not null)
            {
                //Check if this user has such a category (only for non-default ones). Admins can add expenses to any categories
                if (cat.IsDefault == 0 && !isAdmin)
                {
                    found = await _dbContext.UserToCategory.Where(x => x.UserId == _userId && x.CategoryId == cat.CategoryId).AnyAsync();
                }
                else found = true;
            }

            return found;
        }
    }
}