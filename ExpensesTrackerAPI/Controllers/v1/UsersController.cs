using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;

namespace ExpensesTrackerAPI.Controllers.v1
{
    [ApiController]
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
        [Authorize(Roles = "admin")]
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

                    _logger.LogMessage($"[UsersController.GetAllUsers] User data accessed", (int)Helpers.LogLevel.Information, null, $"onlyActive: {onlyActive}", null, _userId);
                    return Ok(resultSet);

                }
                else
                    return StatusCode((int)HttpStatusCode.Forbidden);

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.GetAllUsers] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"onlyActive: {onlyActive}", null, _userId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }


        [HttpGet]
        [Authorize(Roles = "user,admin")]
        [Route("api/v{version:apiVersion}/[controller]/GetUser")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [SwaggerResponse(200, Description = "Ok")]
        public async Task<ActionResult<GetUserResponse>> GetUser(int? userId)
        {
            try
            {
                _userId = GetUserId();
                int userToGet = _userId;
                bool isAdmin = await IsAdmin();
                
                //Admins can request data on any user but regular users must only have access to their own data
                if (isAdmin && userId is not null)
                    userToGet = (int)userId;

                else if (userId is not null && userId != _userId && !isAdmin)
                {
                    _logger.LogMessage($"[UsersController.GetUser] A non-admin attempted to access another user data", (int)Helpers.LogLevel.Information, null, $"userId: {userId}", null, _userId);
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
                    
                var user = _dbContext.Users.Where(c => c.UserId == userToGet).Select(usr => new GetUserResponse
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
                {
                    _logger.LogMessage($"[UsersController.GetUser] User {userToGet} data accessed", (int)Helpers.LogLevel.Information, null, null, null, _userId);
                    return Ok(user);
                }
                    
                else
                {
                    _logger.LogMessage($"[UsersController.GetUser] User {userToGet} not found", (int)Helpers.LogLevel.Information, null, null, null, _userId);
                    return NotFound("User not found");
                }
                    

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.GetUser] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {userId}", null, _userId);
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
        public async Task<ActionResult> DeleteUser(int? userId)
        {
            try
            {
                _userId = GetUserId();
                int userToDelete = _userId;
                bool isAdmin = await IsAdmin();

                //Admins can delete any user but regular users can only delete their own accounts
                if (userId is not null && isAdmin)
                    userToDelete = (int)userId;

                else if (userId is not null && userId != _userId && !isAdmin)
                {
                    _logger.LogMessage($"[UsersController.Delete] A non-admin attempted to delete another user", (int)Helpers.LogLevel.Information, null, $"userId: {userId}",null, _userId);
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
                    
                var user = await _dbContext.Users.Where(x => x.UserId == userToDelete && x.Active == 1).FirstOrDefaultAsync();

                if (user is null)
                {
                    _logger.LogMessage($"[UsersController.Delete] User {userToDelete} not found", (int)Helpers.LogLevel.Information, null, null, null, _userId);
                    return NotFound("User not found");
                }
                    
                else
                {
                    user.Active = 0;
                    _dbContext.Users.Update(user);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogMessage($"[UsersController.Delete] User {userToDelete} deleted", (int)Helpers.LogLevel.Information, null, null, null, _userId);
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.DeleteUser] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId: {userId}", null, _userId);
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
        public async Task<ActionResult<GetUserResponse>> Update(UpdateUserRequest request)
        {
            try
            {
                //All users can update only their own data, admins have no special privileges to change any user data
                
                _userId = GetUserId();
                var user = await _dbContext.Users.Where(x => x.UserId == _userId).FirstOrDefaultAsync();

                if (user is null)
                {
                    _logger.LogMessage($"[UsersController.Update] User {_userId} not found", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, _userId);
                    return NotFound("User not found");
                }
                    

                if (!String.IsNullOrEmpty(request.Email))
                {
                    if (!MailAddress.TryCreate(request.Email, out var email))
                    {
                        _logger.LogMessage($"[UsersController.Update] Invalid e-mail address provided", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, _userId);
                        return BadRequest("Please enter a correct e-mail address");
                    }
                        
                    if (_dbContext.Users.Where(x => x.Email == request.Email).Any())
                    {
                        _logger.LogMessage($"[UsersController.Update] Account with the provided email already exists", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, _userId);
                        return BadRequest("An account with this email already exists");
                    }
                        

                    user.Email = request.Email;
                }
                
                if (!String.IsNullOrEmpty(request.Username))
                {
                    if (_dbContext.Users.Where(x => x.Username == request.Username).Any())
                    {
                        _logger.LogMessage($"[UsersController.Update] Username is taken", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, _userId);
                        return BadRequest("This user name is already taken");
                    }
                        
                    user.Username = request.Username;

                } 

                user.Surname = String.IsNullOrEmpty(request.Surname) ? user.Surname : request.Surname;
                user.Name = String.IsNullOrEmpty(request.Name) ? user.Name : request.Name;
                user.PhoneNumber = String.IsNullOrEmpty(request.PhoneNumber) ? user.PhoneNumber : request.PhoneNumber;

                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogMessage($"[UsersController.Update] User data update performed", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), null, _userId);
                
                return Ok(new GetUserResponse
                {
                    Surname = user.Surname,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Id = user.UserId,
                    AccountType = user.AccountType,
                    Active = user.Active,
                    RegistrationDate = user.RegistrationDate,
                    Username = user.Username
                });
              

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.Update] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request), null, _userId);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPut]
        [Authorize(Roles = "admin")]
        [Route("api/v{version:apiVersion}/[controller]/ChangeAccountType")]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(404, Description = "Not found")]
        public async Task<ActionResult> ChangeAccountType([Required] int userId, UserType accType)
        {
            try
            {
                _userId = GetUserId();
                bool isAdmin = await IsAdmin();
                if (!isAdmin)
                {
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }

                var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserId == userId && x.Active == 1);

                if (user is null)
                {
                    _logger.LogMessage($"[UsersController.ChangeAccountType] User {userId} not found", (int)Helpers.LogLevel.Information, null, $"userId:  {userId}, accType: {accType}", null, _userId);
                    return NotFound("User not found");
                }
                  
                user.AccountType = (int)accType;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogMessage($"[UsersController.ChangeAccountType] User {userId} account type changed", (int)Helpers.LogLevel.Information, null, $"userId: {userId}, accType: {(int)accType}", null, _userId);

                return Ok();


            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[UsersController.ChangeAccountType] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"userId:  {userId}, accType: {accType}", null, _userId);
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
