using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Dapr.Client;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly DaprClient _daprClient;
        private const string STORE_NAME = "statestore";
        private const string USERS_KEY = "users";

        public AuthController(IMediator mediator, DaprClient daprClient)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the existing users from the state store
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME, 
                USERS_KEY
            ) ?? new List<UserData>();

            // Check if username already exists
            if (users.Any(u => string.Equals(u.Username, request.Username, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { success = false, message = "Username already exists" });
            }

            // Hash the password (implement proper hashing in production)
            string passwordHash = HashPassword(request.Password);

            // Create a new user
            var newUser = new UserData
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                IsActive = true,
                Roles = new List<string> { "User" },  // Default role
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Registration"
            };

            // Add roles if specified
            if (request.Roles != null && request.Roles.Any())
            {
                newUser.Roles.AddRange(request.Roles);
                // Remove duplicates
                newUser.Roles = newUser.Roles.Distinct().ToList();
            }

            // Add the new user to the list
            users.Add(newUser);

            // Save the updated list back to the state store
            await _daprClient.SaveStateAsync(
                STORE_NAME,
                USERS_KEY,
                users
            );

            return Ok(new { 
                success = true, 
                message = "Registration successful", 
                username = newUser.Username 
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Get IP address from the current connection
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Create login command
            var command = new LoginCommand
            {
                Username = request.Username,
                Password = request.Password,
                IpAddress = ipAddress
            };

            // Send command through MediatR
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return Unauthorized(new { 
                    success = false, 
                    message = result.Message 
                });
            }

            // Set cookies to maintain session
            HttpContext.Response.Cookies.Append("Username", result.Username, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = result.ExpiryTime
            });

            return Ok(new
            {
                success = true,
                token = result.Token,
                username = result.Username,
                expiryTime = result.ExpiryTime
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            // Get username from claims
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new { success = false, message = "Not logged in" });
            }

            // Get IP address
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Get the existing users
            var users = await _daprClient.GetStateAsync<List<UserData>>(
                STORE_NAME,
                USERS_KEY
            ) ?? new List<UserData>();

            // Find the user
            var user = users.FirstOrDefault(u => 
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user != null && user.IpSessions != null)
            {
                // Remove the IP session for this IP address
                user.IpSessions.RemoveAll(s => s.IpAddress == ipAddress);

                // Save the updated list back to the state store
                await _daprClient.SaveStateAsync(
                    STORE_NAME,
                    USERS_KEY,
                    users
                );
            }

            // Clear cookies
            HttpContext.Response.Cookies.Delete("Username");

            return Ok(new { success = true, message = "Logout successful" });
        }

        [HttpGet("status")]
        public async Task<IActionResult> CheckStatus()
        {
            // Get username from cookies or headers
            string username = null;
            if (HttpContext.Request.Cookies.TryGetValue("Username", out string cookieUsername))
            {
                username = cookieUsername;
            }
            else if (HttpContext.Request.Headers.TryGetValue("X-Username", out var headerUsername))
            {
                username = headerUsername;
            }

            if (string.IsNullOrEmpty(username))
            {
                return Ok(new { isLoggedIn = false, message = "Not logged in" });
            }

            // Get IP address
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Create command to check IP session
            var command = new CheckActiveByIpCommand
            {
                Username = username,
                IpAddress = ipAddress
            };

            // Send command through MediatR
            var result = await _mediator.Send(command);

            return Ok(new
            {
                isLoggedIn = result.Exists && result.IsActive,
                username = username,
                roles = result.Roles,
                message = result.Message,
                expiryTime = result.ExpiryTime
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
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }
        
        public List<string> Roles { get; set; }
    }
}