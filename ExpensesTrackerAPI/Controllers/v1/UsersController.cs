using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;

namespace ExpensesTrackerAPI.Controllers.v1
{
    [ApiController]
    [Authorize(Roles = "admin")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    [SwaggerResponse(401, Description = "Unauthorized")]
    [SwaggerResponse(500, Description = "Internal server error")]
    public class UsersController : ControllerBase
    {
        private readonly IWeblogService _logger;
        private readonly ExpenseDbContext _dbContext;
        private int _userId;

        public UsersController(IWeblogService logger, ExpenseDbContext context)
        {
            _logger = logger;
            _dbContext = context;
        }

        [HttpGet]
        [Route("api/v{version:apiVersion}/[controller]/GetAllUsers")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<List<GetUserResponse>>> GetAllUsers(int? onlyActive = 1)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();
               
                if (isAdmin)
                {
                    var users = _dbContext.Users.OrderBy(x => x.Username); 
                    List<GetUserResponse> resultSet = new List<GetUserResponse>();

                    if (onlyActive == 1)
                        users = (IOrderedQueryable<User>)users.Where(x => x.Active == 1);

                    foreach (var usr in users)
                    {
                        resultSet.Add(new GetUserResponse
                        {
                            Name = usr.Name,
                            Surname = usr.Surname,
                            AccountType = usr.AccountType,
                            Active = usr.Active,
                            RegistrationDate = usr.RegistrationDate,
                            Email = usr.Email,
                            Id = usr.UserId,
                            PhoneNumber = usr.PhoneNumber,
                            Username = usr.Username
                        });
                    }

                    return Ok(resultSet);
                 

                }
                else
                    return StatusCode((int)HttpStatusCode.Unauthorized);

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.GetAllUsers] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {_userId}, onlyActive: {onlyActive}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        [Route("api/v{version:apiVersion}/[controller]/GetUser")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<GetUserResponse>> GetUser([Required]int userId)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();

                if (isAdmin)
                {
                    var user = _dbContext.Users.Where(c => c.UserId == userId).Select(usr => new GetUserResponse
                    {
                        Name = usr.Name,
                        Surname = usr.Surname,
                        AccountType = usr.AccountType,
                        Active = usr.Active,
                        RegistrationDate = usr.RegistrationDate,
                        Email = usr.Email,
                        Id = usr.UserId,
                        PhoneNumber = usr.PhoneNumber,
                        Username = usr.Username
                    }).FirstOrDefault();

                    if (user is not null)
                        return Ok(user);
                    else
                        return NotFound("User not found");


                }
                else
                    return StatusCode((int)HttpStatusCode.Unauthorized);

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.GetUser] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {_userId}");
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
