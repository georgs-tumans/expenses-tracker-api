using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using ExpensesTrackerAPI.Providers;
using ExpensesTrackerAPI.V1_0.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class ExpenseController : ApiControllerBase
    {
        private readonly IWeblogService _logger;
        private readonly ExpenseProvider _expenseProvider;

        public ExpenseController(IWeblogService logger, ExpenseDbContext context) : base(new UserProvider(context), new ControllerHelper(context))
        {
            _logger = logger;
            _expenseProvider = new ExpenseProvider(context);

        }

        [HttpPost]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/Add")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.BadRequest)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(400, Description = "Bad request")]
        public async Task<ActionResult<AddExpenseResponse>> Add(AddExpenseRequest request)
        {
            try
            {
                if (!await _controllerHelper.ValidateCategory((int)request.CategoryId, UserId, IsAdmin))
                {
                    _logger.LogMessage("[ExpenseController.Add] Expense category not found", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);
                    return BadRequest("Such expense category does not exist");
                }

                int newExpenseId = await _expenseProvider.AddNewExpense(request, UserId);

                _logger.LogMessage("[ExpenseController.Add] Expense added", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);

                return Ok(new AddExpenseResponse
                {
                    ExpenseId = newExpenseId
                });
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Add] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), null, UserId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/Update")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        public async Task<ActionResult<Expense>> Update(UpdateExpenseRequest request)
        {
            try
            {
                if (request.CategoryId is not null && !await _controllerHelper.ValidateCategory((int)request.CategoryId, UserId, IsAdmin))
                {
                    _logger.LogMessage("[ExpenseController.Update] Expense category not found", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);
                    return BadRequest("Such expense category does not exist");
                }

                var updatedExpense = await _expenseProvider.UpdateExpense(request, UserId, IsAdmin);
                _logger.LogMessage("[ExpenseController.Update] Expense udpated", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);

                return Ok(updatedExpense);

            }
            catch (ArgumentNullException ex)
            {
                _logger.LogMessage($"[ExpenseController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), null, UserId);
                return NotFound("Expense not found or you have no access to it");
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), null, UserId);
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
                await _expenseProvider.DeleteExpense(expenseId, UserId, IsAdmin);
                _logger.LogMessage($"[ExpenseController.Delete] Expense {expenseId} deleted", (int)Helpers.LogLevel.Information, null, null, null, UserId);

                return Ok();

            }
            catch (ArgumentNullException ex)
            {
                _logger.LogMessage($"[ExpenseController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"expenseId: {expenseId}", null, UserId);
                return NotFound("Expense not found or you have no access to it");
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"expenseId: {expenseId}", null, UserId);
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
                var result = await _expenseProvider.GetAllUserExpenses(UserId, dateFrom, dateTo, category);

                _logger.LogMessage($"[ExpenseController.GetAll] User expenses accessed", (int)Helpers.LogLevel.Information, null, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}", null, UserId);
                return Ok(result);

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAll] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}", null, UserId);
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
                if (IsAdmin)
                {
                    var resultList = await _expenseProvider.GetAllExpenses(dateFrom, dateTo, category);

                    _logger.LogMessage($"[ExpenseController.GetAllAdmin] Expenses accessed", (int)Helpers.LogLevel.Information, null, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}, filterUserId: { filterUserId}", null, UserId);
                    return Ok(resultList);
                }

                else
                {
                    _logger.LogMessage($"[ExpenseController.GetAllAdmin] Unauthorized attempt to access expenses", (int)Helpers.LogLevel.Information, null, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}, filterUserId: { filterUserId}", null, UserId);
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.GetAllAdmin] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"dateFrom: {dateFrom}, dateTo: {dateTo}, category: {category}, filterUserId: { filterUserId}", null, UserId);
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
                var expense = await _expenseProvider.GetUserExpense(expenseId, UserId, IsAdmin);

                if (expense is null)
                {
                    _logger.LogMessage($"[ExpenseController.Get] Expense {expenseId} not found", (int)Helpers.LogLevel.Information, null, null, null, UserId);
                    return NotFound("Expense not found or you have no access to it");
                }
                else
                {
                    _logger.LogMessage($"[ExpenseController.Get] Expense {expenseId} accessed", (int)Helpers.LogLevel.Information, null, null, null, UserId);
                    return Ok(expense);
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[ExpenseController.Get] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"epenseId: {expenseId}", null, UserId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}