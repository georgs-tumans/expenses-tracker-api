using ExpensesTrackerAPI.Models.Database;
using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace ExpensesTrackerAPI.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class AuthController : Controller
    {
        private static IWeblogService _logger;
        private readonly ExpenseDbContext _dbContext;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IWeblogService logger, ExpenseDbContext context, IAuthService authService, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = context;
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(400, Description = "Bad request")]
        [SwaggerResponse(500, Description = "Internal server error")]
        [Route("api/v{version:apiVersion}/[controller]/register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            try
            {
                if (!MailAddress.TryCreate(request.Email, out var email))
                {
                    _logger.LogMessage($"[AuthController.Register] Invalid email address", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("Please enter a correct e-mail address");
                }


                if (!_authService.IsValidPassword(request.Password.Trim()))
                {
                    _logger.LogMessage($"[AuthController.Register] Password does not match requirements", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("Password must contain at least one uppercase character, one number and one special character");
                }


                if (request.Password.Trim() != request.RepeatedPassword.Trim())
                {
                    _logger.LogMessage($"[AuthController.Register] Passwords do not match", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("Passwords do not match");
                }


                if (_dbContext.Users.Where(x => x.Email == request.Email).Any())
                {
                    _logger.LogMessage($"[AuthController.Register] An account with the provided email already exists", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("An account with this email already exists");
                }


                if (_dbContext.Users.Where(x => x.Username == request.UserName).Any())
                {
                    _logger.LogMessage($"[AuthController.Register] Username is taken", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("This user name is already taken");
                }


                _authService.HashPassword(request.Password.Trim(), out byte[] passwordHash, out byte[] passwordSalt);
                User user = new User()
                {
                    Name = request.Name != null ? request.Name.Trim() : null,
                    Surname = request.Surname != null ? request.Surname.Trim() : null,
                    Email = request.Email.Trim(),
                    PhoneNumber = request.PhoneNumber != null ? request.PhoneNumber.Trim() : null,
                    Username = request.UserName.Trim(),
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    Active = 0,
                    AccountType = (int)UserType.User,
                    RegistrationDate = DateTime.UtcNow
                };

                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                _logger.LogMessage($"[AuthController.Register] User created", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), $"New user id: {user.UserId}");

                string ctoken = Guid.NewGuid().ToString();
                user.EmailConfirmationToken = ctoken;
                user.EmailConfirmationTokenRegistration = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                string link = Url.ActionLink("ConfirmEmail", "Auth", new { id = user.UserId, token = ctoken }, protocol: HttpContext.Request.Scheme);
                if (link is not null)
                {
                    _authService.SendConfirmationEmail(ctoken, link, user);
                    _logger.LogMessage($"[AuthController.Register] Email confirmation link sent to {user.Email}", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                }
                    
                else
                {
                    _logger.LogMessage($"[AuthController.Register] Failed to generate email confirmation link", (int)Helpers.LogLevel.Error, null, JsonSerializer.Serialize(request));
                    return StatusCode((int)HttpStatusCode.InternalServerError);
                }
                    
                return Ok();

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[AuthController.Register] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, JsonSerializer.Serialize(request));
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(byte[]), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        [SwaggerResponse(200, Description = "Ok")]
        [SwaggerResponse(400, Description = "Bad request")]
        [SwaggerResponse(403, Description = "Forbidden")]
        [SwaggerResponse(404, Description = "Not found")]
        [SwaggerResponse(500, Description = "Internal server error")]
        [Route("api/v{version:apiVersion}/[controller]/login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginUserRequest request)
        {
            try
            {
                if (String.IsNullOrEmpty(request.AuthString))
                {
                    _logger.LogMessage($"[AuthController.Login] Auth string not provided", (int)Helpers.LogLevel.Information, null, $"User: {request.AuthString}");
                    return BadRequest("Either e-mail or user name must be provided");
                }

                else
                {
                    var user = _dbContext.Users.Where(x => (x.Username == request.AuthString.Trim() || x.Email == request.AuthString.Trim())).FirstOrDefault();

                    if (user is null)
                    {
                        _logger.LogMessage($"[AuthController.Login] User not found", (int)Helpers.LogLevel.Information, null, $"User: {request.AuthString}");
                        return NotFound("User not found");
                    }

                    if (user.Active == 0)
                    {
                        _logger.LogMessage($"[AuthController.Login] User {user.UserId} account has not been activated yet", (int)Helpers.LogLevel.Information, null, $"User: {request.AuthString}");
                        return StatusCode((int)HttpStatusCode.Forbidden, "This account has not been activated");
                    }

                    else
                    {
                        bool verified = _authService.VerifyPasswordHash(request.Password.Trim(), user.PasswordHash, user.PasswordSalt);
                        if (verified)
                        {
                            UserType type = (UserType)user.AccountType;
                            string token = _authService.CreateToken(user, _configuration.GetSection("AuthTokenKey").Value, type);
                            LoginResponse response = new LoginResponse
                            {
                                Id = user.UserId,
                                Username = user.Username,
                                Email = user.Email,
                                Name = user.Name,
                                Surname = user.Surname,
                                AccountType = user.AccountType,
                                PhoneNumber = user.PhoneNumber,
                                RegistrationDate = user.RegistrationDate,
                                JwtToken = token
                            };

                            _logger.LogMessage($"[AuthController.Login] User logged in", (int)Helpers.LogLevel.Information, null, $"User: {request.AuthString}");
                            return Ok(response);
                        }
                        else
                        {
                            _logger.LogMessage($"[AuthController.Login] Invalid password", (int)Helpers.LogLevel.Information, null, $"User: {request.AuthString}");
                            return BadRequest("Incorrect password");
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[AuthController.Login] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"User: {request.AuthString}");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("api/v{version:apiVersion}/[controller]/confirmEmail")]
        public async Task<ActionResult> ConfirmEmail(int id, string token)
        {
            try
            {
                if (id == 0 || String.IsNullOrEmpty(token))
                {
                    _logger.LogMessage($"[AuthController.ConfirmEmail] Invalid incoming parameters", (int)Helpers.LogLevel.Information, null, $"user id: {id}, token: {token}");
                    return Redirect(_configuration.GetSection("EmailConfirmFailUrl").Value);
                }

                var user = await _dbContext.Users.FindAsync(id);
                if (user is null)
                {
                    _logger.LogMessage($"[AuthController.ConfirmEmail] Could not find the user", (int)Helpers.LogLevel.Information, null, $"user id: {id}, token: {token}");
                    return Redirect(_configuration.GetSection("EmailConfirmFailUrl").Value);
                }

                int tokenExpiration = Convert.ToInt32(_configuration.GetSection("TokenExpirationHours").Value);
                if (DateTime.UtcNow.Subtract((DateTime)user.EmailConfirmationTokenRegistration).Hours >= tokenExpiration)
                {
                    _logger.LogMessage($"[AuthController.ConfirmEmail] Email confirmation token has expired", (int)Helpers.LogLevel.Information, null, $"user id: {id}, token: {token}");
                    return Redirect(_configuration.GetSection("EmailConfirmFailUrl").Value);
                }

                bool isSuccess = user.EmailConfirmationToken.Equals(token);

                if (isSuccess)
                {
                    user.Active = 1;
                    await _dbContext.SaveChangesAsync();
                    _logger.LogMessage($"[AuthController.ConfirmEmail] User {id} email '{user.Email}' confirmed and account activated", (int)Helpers.LogLevel.Information, null, $"user id: {id}, token: {token}");
                    return Redirect(_configuration.GetSection("EmailConfirmSuccessUrl").Value);
                }
                else
                {
                    _logger.LogMessage($"[AuthController.ConfirmEmail] Tokens do not match", (int)Helpers.LogLevel.Information, null, $"user id: {id}, token: {token}");
                    return Redirect(_configuration.GetSection("EmailConfirmFailUrl").Value);
                }

            }
            catch (Exception ex)
            {
                _logger.LogMessage($"[AuthController.ConfirmEmail] {ex.Message}", (int)Helpers.LogLevel.Error, ex.StackTrace, $"user id: {id}, token: {token}");
                return Redirect(_configuration.GetSection("EmailConfirmFailUrl").Value);
            }

        }
    }
}
