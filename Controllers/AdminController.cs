// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using Think4.Services;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Dapr.Client;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,Manager")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string USERS_KEY = "users";

    public AdminController(IAuthService authService, DaprClient daprClient)
    {
        _authService = authService;
        _daprClient = daprClient;
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
            
            return Ok(ApiResponse<List<UserDto>>.CreateSuccess(userDtos, "Lấy danh sách người dùng thành công!"));
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
            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Lấy thông tin người dùng thành công!"));
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
            
            return Ok(ApiResponse<UserDto>.CreateSuccess(userDto, "Tạo tài khoản quản lý thành công!"));
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
}