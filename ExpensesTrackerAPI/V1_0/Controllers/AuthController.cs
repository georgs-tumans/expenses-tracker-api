using ExpensesTrackerAPI.Models.Requests;
using ExpensesTrackerAPI.Models.Responses;
using ExpensesTrackerAPI.V1_0.Controllers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Text.Json;

namespace ExpensesTrackerAPI.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Produces("application/json")]
    public class AuthController : ApiControllerBase
    {
        private static IWeblogService _logger;
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly LinkGenerator _linkGenerator;

        public AuthController(IWeblogService logger, 
                              IAuthService authService, 
                              IConfiguration configuration, 
                              ExpenseDbContext context, 
                              LinkGenerator linkGenerator) : base(new Providers.UserProvider(context), new ControllerHelper(context))
        {
            _logger = logger;
            _authService = authService;
            _configuration = configuration;
            _linkGenerator = linkGenerator;
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
                if (!_controllerHelper.ValidateEmail(request.Email))
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

                if (await _userProvider.GetUserByEmailAsync(request.Email) is not null)
                {
                    _logger.LogMessage($"[AuthController.Register] An account with the provided email already exists", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("An account with this email already exists");
                }


                if (await _userProvider.GetUserByUsernameAsync(request.UserName) is not null)
                {
                    _logger.LogMessage($"[AuthController.Register] Username is taken", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
                    return BadRequest("This user name is already taken");
                }

                var newUser = await _userProvider.RegisterNewUserAsync(request, _authService, _linkGenerator, HttpContext.Request.HttpContext);
                _logger.LogMessage($"[AuthController.Register] User created", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request), $"New user id: {newUser.UserId}");
                _logger.LogMessage($"[AuthController.Register] Email confirmation link sent to {newUser.Email}", (int)Helpers.LogLevel.Information, null, JsonSerializer.Serialize(request));
               
                return Ok();

            }
            catch(ArgumentNullException ex)
            {
                _logger.LogMessage($"[AuthController.Register] Failed to generate email confirmation link", (int)Helpers.LogLevel.Error, null, JsonSerializer.Serialize(request));
                return StatusCode((int)HttpStatusCode.InternalServerError);
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
                    var user = await _userProvider.GetUserByAuthStringAsync(request.AuthString);

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

                var user = await _userProvider.GetUserAsync(id);
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
                   await _userProvider.ActivateUserAsync(user);
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
