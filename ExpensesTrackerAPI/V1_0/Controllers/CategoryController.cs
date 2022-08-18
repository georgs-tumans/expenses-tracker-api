using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Database;
using System.Text.Json;
using ExpensesTrackerAPI.Models.Responses;
using System.ComponentModel.DataAnnotations;
using ExpensesTrackerAPI.V1_0.Controllers;
using ExpensesTrackerAPI.Providers;

namespace ExpensesTrackerAPI.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    [SwaggerResponse(401, Description = "Unauthorized")]
    [SwaggerResponse(500, Description = "Internal server error")]
    public class CategoryController : ApiControllerBase
    {
        private readonly IWeblogService _logger;
        private readonly CategoryProvider _categoryProvider;

        public CategoryController(IWeblogService logger, ExpenseDbContext context) : base(new UserProvider(context), new ControllerHelper(context))
        {
            _logger = logger;
            _categoryProvider = new CategoryProvider(context);
        }


        [HttpPost]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.BadRequest)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(400, Description = "Bad request")]
        public async Task<ActionResult<int>> Add(AddCategoryRequest request)
        {
            try
            {
                if (request.IsDefault is not null && request.IsDefault == 1 && !IsAdmin)
                {
                    _logger.LogMessage($"[CategoryController.Add] Unauthorized attempt to add a default category", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);
                    return BadRequest("Only administrators can add default categories");
                }

                int newCatId = await _categoryProvider.AddNewCategoryAsync(request, UserId);

                _logger.LogMessage($"[CategoryController.Add] New category added", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);
                return Ok(newCatId);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.Add] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), null, UserId);
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
        public async Task<ActionResult<ExpenseCategory>> Update(UpdateCategoryRequest request)
        {
            try
            {
                bool isEditable = false;

                var category = await _categoryProvider.GetCategoryAsync(request.Id);
                if (category is null)
                {
                    _logger.LogMessage($"[CategoryController.Update] Category {request.Id} not found", (int)Helpers.LogLevel.Error, null, JsonSerializer.Serialize(request), null, UserId);
                    return BadRequest($"Category {request.Id} not found");
                }

                if (category.IsDefault == 1 && !IsAdmin)
                {
                    _logger.LogMessage("[CategoryController.Update] Only administrators can edit default categories", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);
                    return BadRequest("Only administrators can edit default categories");
                }

                isEditable = await _categoryProvider.CheckIfUserHasCategoryAsync(request.Id, UserId) || IsAdmin;  //Admin users should be able to update any category

                if (!isEditable)
                {
                    _logger.LogMessage($"[CategoryController.Update] Category {request.Id} cannot be updated", (int)Helpers.LogLevel.Error, null, JsonSerializer.Serialize(request), null, UserId);
                    return BadRequest($"Category {request.Id} cannot be updated");
                }

                await _categoryProvider.UpdateCategoryAsync(request, category);
                _logger.LogMessage($"[CategoryController.Update] Category {category.CategoryId} updated", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, UserId);

                return Ok(category);

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), null, UserId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetAll")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<List<GetAllUserCategoriesResponse>>> GetAll()
        {
            try
            {
                //Selects all active user defined categories as well as all active default categories (since those are pre-made for every user)
                var resultSet = await _categoryProvider.GetAllUserCategoriesAsync(UserId);
                _logger.LogMessage($"[CategoryController.GetAll] User categories accessed", (int)Helpers.LogLevel.Information, null, null, null, UserId);

                return Ok(resultSet);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.GetAll] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, null, null, UserId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetAllAdmin")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<List<GetAllUserCategoriesResponse>>> GetAllAdmin(int? userId, int IncludeDefault = 1)
        {
            try
            {
                if (IsAdmin)
                {

                    IEnumerable<GetAllUserCategoriesResponse> result;

                    if (userId is not null && userId != 0)
                    {
                        //Selects all active user defined categories as well as all active default categories (since those are pre-made for every user) of the passed in user
                        result = await _categoryProvider.GetAllUserCategoriesAsync((int)userId);
                    }
                    else
                    {
                        //Select all active categories of all users
                        result = await _categoryProvider.GetAllActiveCategoriesAsync();
                    }

                    if (IncludeDefault == 0)
                    {
                        result = result.Where(x => x.Default == 0).ToList();
                    }

                    _logger.LogMessage($"[CategoryController.GetAllAdmin] Categories accessed", (int)Helpers.LogLevel.Information, null, $"userId: {UserId}, IncludeDefault: {IncludeDefault}", null, UserId);
                    return Ok(result);
                }

                else
                {
                    _logger.LogMessage($"[CategoryController.GetAllAdmin] Unauthorized attempt to access categories", (int)Helpers.LogLevel.Information, null, $"userId: {UserId}, IncludeDefault: {IncludeDefault}", null, UserId);
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.GetAllAdmin] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {UserId}, IncludeDefault: {IncludeDefault}", null, UserId);
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
        public async Task<ActionResult> Delete([Required] int categoryId)
        {
            try
            {
                var dbCategory = await _categoryProvider.GetCategoryAsync(categoryId);
                bool isEditable = await _categoryProvider.CheckIfUserHasCategoryAsync(categoryId, UserId) || IsAdmin; //Admin users should be able to delete any category 

                if (dbCategory is not null && dbCategory.IsDefault == 1 && !IsAdmin)
                {
                    _logger.LogMessage("[CategoryController.Delete] Only administrators can delete default categories", (int)Helpers.LogLevel.Information, null, $"Category id: {categoryId}", null, UserId);
                    return BadRequest("Only administrators can delete default categories");
                }

                if (dbCategory is null || !isEditable)
                {
                    _logger.LogMessage("[CategoryController.Delete] Category not found", (int)Helpers.LogLevel.Information, null, $"Category id: {categoryId}", null, UserId);
                    return NotFound("Category not found");
                }

                await _categoryProvider.DeleteCategoryAsync(dbCategory);
                _logger.LogMessage("[CategoryController.Delete] Category deleted", (int)Helpers.LogLevel.Information, null, $"Category id: {categoryId}", null, UserId);
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"Category id: {categoryId}", null, UserId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
