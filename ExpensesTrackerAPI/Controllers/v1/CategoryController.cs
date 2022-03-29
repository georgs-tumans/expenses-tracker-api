using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Database;
using System.Text.Json;
using ExpensesTrackerAPI.Models.Responses;
using System.ComponentModel.DataAnnotations;

namespace ExpensesTrackerAPI.Controllers.v1
{
    [ApiController]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    [SwaggerResponse(401, Description = "Unauthorized")]
    [SwaggerResponse(500, Description = "Internal server error")]
    public class CategoryController : ControllerBase
    {
        private readonly IWeblogService _logger;
        private readonly ExpenseDbContext _dbContext;
        private int _userId;

        public CategoryController(IWeblogService logger, ExpenseDbContext context)
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
        public async Task<ActionResult<int>> Add(AddCategoryRequest request)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();

                if (request.IsDefault is not null && request.IsDefault == 1 && !isAdmin)
                    return BadRequest("Only administrators can add default categories");

                var newCat = new ExpenseCategory
                {
                    Name = request.Name,
                    Description = request.Description,
                    Active = 1,
                    IsDefault = request.IsDefault is not null ? (int)request.IsDefault : 0
                };

                _dbContext.ExpensesCategories.Add(newCat);
                await _dbContext.SaveChangesAsync();

                if (newCat.IsDefault == 0) //non-default categories are tied to individual users who create them
                {
                    _dbContext.UserToCategory.Add(new UserToCategory
                    {
                        CategoryId = newCat.CategoryId,
                        UserId = _userId
                    });
                    await _dbContext.SaveChangesAsync();
                }
                
                return Ok(newCat.CategoryId);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.Add] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), $"Current user id: {_userId}");
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
        public async Task<ActionResult> Update(UpdateCategoryRequest request)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();
                bool isEditable = false;

                var dbCategory = await _dbContext.ExpensesCategories.Where(x => x.CategoryId == request.Id).FirstOrDefaultAsync();
                isEditable = await _dbContext.UserToCategory.Where(x => x.UserId == _userId && x.CategoryId == request.Id).AnyAsync() || isAdmin;  //Admin users should be able to update any category

                if (dbCategory is not null && dbCategory.IsDefault == 1 && !isAdmin)
                {
                    _logger.LogMessage("[CategoryController.Update] Only administrators can edit default categories", (int)Helpers.LogLevel.Information, null, $"id: {request.Id}, userId: {_userId}");
                    return BadRequest("Only administrators can edit default categories");
                }

                if (dbCategory is null || !isEditable)
                {
                    _logger.LogMessage("[CategoryController.Update] Category not found", (int)Helpers.LogLevel.Information, null, $"id: {request.Id}, userId: {_userId}");
                    return NotFound("Category not found");
                }

                
                dbCategory.Description = request.Description;
                dbCategory.Name = request.Name;

                _dbContext.ExpensesCategories.Update(dbCategory);
                await _dbContext.SaveChangesAsync();
                return Ok();
                

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), $"Current user id: {_userId}");
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
                _userId = GetUserId();
                List<GetAllUserCategoriesResponse> resultSet = new List<GetAllUserCategoriesResponse>();

                //Selects all active user defined categories as well as all active default categories (since those are pre-made for every user)
                resultSet = await _dbContext.ExpensesCategories.Where(x => x.Active == 1).Join(_dbContext.UserToCategory.Where(x => x.UserId == _userId),
                                                                        category => category.CategoryId,
                                                                        utc => utc.CategoryId,
                                                                        (category, utc) => new GetAllUserCategoriesResponse
                                                                        {
                                                                            CategoryId = category.CategoryId,
                                                                            Name = category.Name,
                                                                            Description = category.Description,
                                                                            Default = category.IsDefault
                                                                        }).Union(_dbContext.ExpensesCategories.Where(x => x.IsDefault == 1 && x.Active == 1).Select(c => new GetAllUserCategoriesResponse {
                                                                            CategoryId = c.CategoryId,
                                                                            Name = c.Name,
                                                                            Description = c.Description,
                                                                            Default=c.IsDefault
                                                                        })).ToListAsync();

                return Ok(resultSet);
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.GetAll] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {_userId}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetAllAdmin")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<List<GetAllUserCategoriesResponse>>> GetAllAdmin(int? UserId, int IncludeDefault = 1)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();

                if (isAdmin)
                {

                    IEnumerable<GetAllUserCategoriesResponse> result;
                    

                    if (UserId is not null && UserId != 0)
                    {
                        //Selects all active user defined categories as well as all active default categories (since those are pre-made for every user)
                        result = await (from cat in _dbContext.ExpensesCategories
                                 join utc in _dbContext.UserToCategory on cat.CategoryId equals utc.CategoryId
                                 where cat.Active == 1 && utc.UserId == UserId
                                 select new GetAllUserCategoriesResponse
                                 {
                                     CategoryId = cat.CategoryId,
                                     Name = cat.Name,
                                     Description = cat.Description,
                                     Default = cat.IsDefault
                                 }).Union(_dbContext.ExpensesCategories.Where(x => x.IsDefault == 1).Select(c => new GetAllUserCategoriesResponse
                                 {
                                     CategoryId = c.CategoryId,
                                     Name = c.Name,
                                     Description = c.Description,
                                     Default = c.IsDefault
                                 })).ToListAsync();
                    }
                    else
                    {
                        //Select all active categories of all users
                        result = await (from cat in _dbContext.ExpensesCategories
                                 where cat.Active == 1
                                 select new GetAllUserCategoriesResponse
                                 {
                                     CategoryId = cat.CategoryId,
                                     Name = cat.Name,
                                     Description = cat.Description,
                                     Default = cat.IsDefault
                                 }).ToListAsync();
                    }
                    

                    if (IncludeDefault == 0)
                    {
                        result = result.Where(x => x.Default == 0).ToList();
                    }

                    return Ok(result);
                }

                else return StatusCode((int)HttpStatusCode.Forbidden);

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.GetAllAdmin] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {_userId}, IncludeDefault: {IncludeDefault}");
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
        public async Task<ActionResult> Delete([Required] int  categoryId)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();
                var dbCategory = await _dbContext.ExpensesCategories.Where(x => x.CategoryId == categoryId).FirstOrDefaultAsync();
                bool isEditable = await _dbContext.UserToCategory.Where(x => x.UserId == _userId && x.CategoryId == categoryId).AnyAsync() || isAdmin;  //Admin users should be able to delete any category

                if (dbCategory is not null && dbCategory.IsDefault == 1 && !isAdmin)
                {
                    _logger.LogMessage("[CategoryController.Delete] Only administrators can delete default categories", (int)Helpers.LogLevel.Information, null, $"Category id: {categoryId}, userId: {_userId}");
                    return BadRequest("Only administrators can delete default categories");
                }

                if (dbCategory is null || !isEditable)
                {
                    _logger.LogMessage("[CategoryController.Delete] Category not found", (int)Helpers.LogLevel.Information, null, $"Category id: {categoryId}, userId: {_userId}");
                    return NotFound("Category not found");
                }

                dbCategory.Active = 0;
                _dbContext.ExpensesCategories.Update(dbCategory);
                await _dbContext.SaveChangesAsync();
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[CategoryController.Delete] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"Category id: {categoryId}, userId: {_userId}");
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
    }
}
