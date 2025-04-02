using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapr.Client;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Restrict to admin role
    public class UsersController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        private const string STORE_NAME = "statestore";
        private const string USERS_KEY = "users";

        public UsersController(DaprClient daprClient)
        {
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            ) ?? new List<UserData>();

            // Return users without password hashes
            var sanitizedUsers = users.Select(u => new
            {
                u.Username,
                u.IsActive,
                u.Roles,
                u.CreatedAt,
                u.CreatedBy,
                IpSessions = u.IpSessions?.Select(s => new
                {
                    s.IpAddress,
                    s.LoginTime,
                    s.ExpiryTime,
                    s.IsActive
                })
            });

            return Ok(sanitizedUsers);
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser(string username)
        {
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            ) ?? new List<UserData>();

            var user = users.FirstOrDefault(u => 
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Return user without password hash
            var sanitizedUser = new
            {
                user.Username,
                user.IsActive,
                user.Roles,
                user.CreatedAt,
                user.CreatedBy,
                IpSessions = user.IpSessions?.Select(s => new
                {
                    s.IpAddress,
                    s.LoginTime,
                    s.ExpiryTime,
                    s.IsActive
                })
            };

            return Ok(sanitizedUser);
        }

        [HttpPut("{username}")]
        public async Task<IActionResult> UpdateUser(string username, [FromBody] UpdateUserRequest request)
        {
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            ) ?? new List<UserData>();

            var user = users.FirstOrDefault(u => 
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user properties
            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            if (request.Roles != null)
            {
                user.Roles = request.Roles;
            }

            // Save the updated user list
            await _daprClient.SaveStateAsync(
                STORE_NAME,
                USERS_KEY,
                users
            );

            return Ok(new
            {
                success = true,
                message = "User updated successfully",
                username = user.Username
            });
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            ) ?? new List<UserData>();

            var userIndex = users.FindIndex(u => 
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (userIndex == -1)
            {
                return NotFound(new { message = "User not found" });
            }

            // Remove the user
            users.RemoveAt(userIndex);

            // Save the updated user list
            await _daprClient.SaveStateAsync(
                STORE_NAME,
                USERS_KEY,
                users
            );

            return Ok(new
            {
                success = true,
                message = "User deleted successfully"
            });
        }

        [HttpPost("{username}/reset-password")]
        public async Task<IActionResult> ResetPassword(string username, [FromBody] ResetPasswordRequest request)
        {
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            ) ?? new List<UserData>();

            var user = users.FirstOrDefault(u => 
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Hash the new password
            user.PasswordHash = HashPassword(request.NewPassword);

            // Clear existing IP sessions to force re-login
            user.IpSessions?.Clear();

            // Save the updated user list
            await _daprClient.SaveStateAsync(
                STORE_NAME,
                USERS_KEY,
                users
            );

            return Ok(new
            {
                success = true,
                message = "Password reset successfully"
            });
        }

        [HttpPost("initialize")]
        [AllowAnonymous] // Allow this method to be called without authentication (only for first-time setup)
        public async Task<IActionResult> InitializeAdminUser([FromBody] InitializeAdminRequest request)
        {
            // Check if users already exist
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            );

            if (users != null && users.Any())
            {
                return BadRequest(new { message = "Users already initialized" });
            }

            // Create initial admin user
            var adminUser = new UserData
            {
                Username = request.Username,
                PasswordHash = HashPassword(request.Password),
                IsActive = true,
                Roles = new List<string> { "Admin", "User" },
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System Initialization"
            };

            // Initialize users list with admin
            users = new List<UserData> { adminUser };

            // Save to state store
            await _daprClient.SaveStateAsync(
                STORE_NAME,
                USERS_KEY,
                users
            );

            return Ok(new
            {
                success = true,
                message = "Admin user initialized successfully"
            });
        }

        // Helper method to hash passwords - replace with proper implementation in production
        private string HashPassword(string password)
        {
            // In real-world implementation, use BCrypt or similar
            // Example: return BCrypt.Net.BCrypt.HashPassword(password);
            return password + "_hashed";
        }
    }

    // Request models
    public class UpdateUserRequest
    {
        public bool? IsActive { get; set; }
        public List<string> Roles { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }

    public class InitializeAdminRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
    }
}