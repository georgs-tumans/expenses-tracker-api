﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Database;
using System.Text.Json;

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
