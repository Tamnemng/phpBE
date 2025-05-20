// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Dapr.Client;
using MediatR;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Manager")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly DaprClient _daprClient;
    private readonly IMediator _mediator;
    private const string STORE_NAME = "statestore";
    private const string USERS_KEY = "users";

    public AdminController(IAuthService authService, DaprClient daprClient, IMediator mediator)
    {
        _authService = authService;
        _daprClient = daprClient;
        _mediator = mediator; // Khởi tạo _mediator
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _daprClient.GetStateAsync<List<User>>(
                STORE_NAME,
                USERS_KEY,
                consistencyMode: ConsistencyMode.Strong
            ) ?? new List<User>();

            var userDtos = users.Select(u => new UserDto(u)).ToList();

            return Ok(ApiResponse<List<UserDto>>.CreateSuccess(userDtos, "Get all users successfully!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        try
        {
            var user = await _authService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.CreateError(
                    $"User with ID {userId} not found",
                    HttpStatusCode.NotFound,
                    "USER_NOT_FOUND"
                ));
            }

            var userDto = new UserDto(user);
            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Get user successfully!"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }

    // For admin to create a manager account directly
    [Authorize(Roles = "Admin")]
    [HttpPost("create-manager")]
    public async Task<IActionResult> CreateManager([FromBody] ManagerRegisterDto registerDto)
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
            // Force role to Manager
            // registerDto.Role = UserRole.Manager;

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

    [Authorize(Roles = "Admin")]
    [HttpPost("users/delete-multiple")]
    public async Task<IActionResult> DeleteMultipleUsers([FromBody] DeleteUsersDto deleteDto)
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
            var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var command = new DeleteUsersCommand(deleteDto.UserIds, username);
            var result = await _mediator.Send(command);

            // Build response based on success and failures
            var responseData = new
            {
                SuccessCount = result.SuccessCount,
                FailedIds = result.FailedIds,
                FailureReasons = result.FailureReasons
            };

            string message = result.SuccessCount > 0
                ? $"Deleted {result.SuccessCount} user(s) successfully"
                : "No users were deleted";

            if (result.FailedIds.Any())
            {
                message += $". {result.FailedIds.Count} users cannot be deleted.";
            }

            return Ok(ApiResponse<object>.CreateSuccess(responseData, message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.BadRequest, "DELETE_ERROR"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message, HttpStatusCode.InternalServerError, "SERVER_ERROR"));
        }
    }
}