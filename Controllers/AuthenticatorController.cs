// Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var user = await _authService.Authenticate(loginDto.Username, loginDto.Password);

            if (user == null)
                return Unauthorized(ApiResponse<object>.CreateError("Invalid username or password", HttpStatusCode.Unauthorized, "AUTH_ERROR"));

            var token = _authService.GenerateJwtToken(user);

            var response = new LoginResponseDto
            {
                Token = token,
                User = new UserDto(user),
                Expiration = DateTime.UtcNow.AddDays(7)
            };

            return Ok(ApiResponse<LoginResponseDto>.CreateSuccess(response, "Login successful!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            // Ensure regular registration always creates a standard user
            // registerDto.Role = UserRole.User;

            var user = await _authService.Register(registerDto);

            var userDto = new UserDto(user);

            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Registration successful!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "REGISTER_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("manager/register")]
    public async Task<IActionResult> RegisterManager([FromBody] ManagerRegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();

            var errorMessage = string.Join("; ", errors);

            return BadRequest(ApiResponse<object>.CreateError(
                errorMessage,
                HttpStatusCode.BadRequest,
                "VALIDATION_ERROR"
            ));
        }

        try
        {
            var user = await _authService.RegisterManager(registerDto);

            var userDto = new UserDto(user);

            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Manager account registration successful!"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "REGISTER_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpGet("check-session")]
    public async Task<IActionResult> CheckSession()
    {
        try
        {
            // Get user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<object>.CreateError("Unauthorized", HttpStatusCode.Unauthorized, "AUTH_ERROR"));

            // Check if session is active
            var isActive = await _authService.IsSessionActive(userId);
            if (!isActive)
                return Unauthorized(ApiResponse<object>.CreateError("Session expired", HttpStatusCode.Unauthorized, "SESSION_EXPIRED"));

            // Update last active timestamp
            await _authService.UpdateUserLastActive(userId);

            // Return user info
            var user = await _authService.GetUserById(userId);
            var userDto = new UserDto(user);

            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Session active"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get user ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse<object>.CreateError("Unauthorized", HttpStatusCode.Unauthorized, "AUTH_ERROR"));

            // Invalidate the session
            await _authService.LogoutUser(userId);

            return Ok(ApiResponse<object>.CreateSuccess(null, "Logout successful!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.CreateError("Unauthorized", HttpStatusCode.Unauthorized, "AUTH_ERROR"));
            }

            var user = await _authService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.CreateError(
                    $"User not found",
                    HttpStatusCode.NotFound,
                    "USER_NOT_FOUND"
                ));
            }

            var userDto = new UserDto(user);
            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Current user details retrieved successfully!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize]
    [HttpPut("me/update")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UserUpdateDto userUpdateDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => $"{e.Key}: {e.Value.Errors.First().ErrorMessage}")
                .ToList();
            var errorMessage = string.Join("; ", errors);
            return BadRequest(ApiResponse<object>.CreateError(errorMessage, HttpStatusCode.BadRequest, "VALIDATION_ERROR"));
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<object>.CreateError("Unauthorized", HttpStatusCode.Unauthorized, "AUTH_ERROR"));
            }

            var updatedUser = await _authService.UpdateUserAsync(userId, userUpdateDto);
            if (updatedUser == null)
            {
                 return NotFound(ApiResponse<object>.CreateError(
                    $"User not found",
                    HttpStatusCode.NotFound,
                    "USER_NOT_FOUND"
                ));
            }
            
            var userDto = new UserDto(updatedUser);
            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "User information updated successfully!"));
        }
        catch (InvalidOperationException ex) // Catches specific errors like "Email already taken"
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "UPDATE_USER_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}