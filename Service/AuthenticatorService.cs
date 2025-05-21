// Services/AuthService.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Dapr.Client;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Think4.Services
{
    public interface IAuthService
    {
        Task<User> Authenticate(string username, string password);
        Task<User> Register(UserRegisterDto userDto);
        Task<User> RegisterManager(ManagerRegisterDto managerDto);
        Task<User> GetUserById(string id);
        Task<User> GetUserByUsername(string username);
        Task UpdateUserLastActive(string userId);
        string GenerateJwtToken(User user);
        Task<bool> IsSessionActive(string userId);
        Task<bool> DeleteUser(string userId);
        Task<(int SuccessCount, List<string> FailedIds)> DeleteUsers(List<string> userIds);
        Task LogoutUser(string userId);
        Task<User?> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto);
    }

    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly DaprClient _daprClient;
        private const string STORE_NAME = "statestore";
        private const string USERS_KEY = "users";
        private static readonly TimeSpan SessionTimeout = TimeSpan.FromHours(1);

        public AuthService(IConfiguration configuration, DaprClient daprClient)
        {
            _configuration = configuration;
            _daprClient = daprClient;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = await GetUserByUsername(username);

            // If no user found or password doesn't match
            if (user == null || !VerifyPasswordHash(password, user.Password))
                return null;

            // Update last active time
            user.UpdateLastActive();
            await SaveUser(user);

            return user;
        }

        public async Task<User> Register(UserRegisterDto userDto)
        {
            if (await GetUserByUsername(userDto.Username) != null)
                throw new InvalidOperationException($"Username '{userDto.Username}' is already taken");

            var users = await GetAllUsers();

            // Create new user
            var user = new User(
                userDto.Username,
                HashPassword(userDto.Password),
                userDto.Email,
                userDto.FullName,
                UserRole.User
            );

            users.Add(user);

            await _daprClient.SaveStateAsync(STORE_NAME, USERS_KEY, users);

            return user;
        }

        public async Task<User> RegisterManager(ManagerRegisterDto managerDto)
        {

            if (await GetUserByUsername(managerDto.Username) != null)
                throw new InvalidOperationException($"Username '{managerDto.Username}' is already taken");

            var users = await GetAllUsers();

            // Create new manager user
            var user = new User(
                managerDto.Username,
                HashPassword(managerDto.Password),
                managerDto.Email,
                managerDto.FullName,
                UserRole.Manager
            );

            users.Add(user);

            await _daprClient.SaveStateAsync(STORE_NAME, USERS_KEY, users);

            return user;
        }

        public async Task<User> GetUserById(string id)
        {
            var users = await GetAllUsers();
            return users.FirstOrDefault(u => u.Id == id);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var users = await GetAllUsers();
            return users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        public async Task UpdateUserLastActive(string userId)
        {
            var user = await GetUserById(userId);
            if (user != null)
            {
                user.UpdateLastActive();
                await SaveUser(user);
            }
        }

        public async Task<bool> IsSessionActive(string userId)
        {
            var user = await GetUserById(userId);
            if (user == null)
                return false;

            return !user.IsSessionExpired(SessionTimeout);
        }

        public async Task LogoutUser(string userId)
        {
            var user = await GetUserById(userId);
            if (user != null)
            {
                // Set last active to null to invalidate the session
                user.LastActive = null;
                await SaveUser(user);
            }
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? "YourVeryLongSecretKeyHereForThink4ApiSecurity123!");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Token valid for 7 days
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<List<User>> GetAllUsers()
        {
            return await _daprClient.GetStateAsync<List<User>>(
                STORE_NAME,
                USERS_KEY,
                consistencyMode: ConsistencyMode.Strong
            ) ?? new List<User>();
        }

        private async Task SaveUser(User user)
        {
            var users = await GetAllUsers();

            var existingUserIndex = users.FindIndex(u => u.Id == user.Id);
            if (existingUserIndex >= 0)
            {
                users[existingUserIndex] = user;
            }
            else
            {
                users.Add(user);
            }

            await _daprClient.SaveStateAsync(STORE_NAME, USERS_KEY, users);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Simple comparison for our example
            var passwordHash = HashPassword(password);
            return passwordHash == storedHash;
        }

        public async Task<bool> DeleteUser(string userId)
        {
            var users = await GetAllUsers();

            int initialCount = users.Count;
            users.RemoveAll(u => u.Id == userId);

            if (users.Count == initialCount)
                return false;

            await _daprClient.SaveStateAsync(STORE_NAME, USERS_KEY, users);
            return true;
        }

        public async Task<(int SuccessCount, List<string> FailedIds)> DeleteUsers(List<string> userIds)
        {
            var users = await GetAllUsers();
            var failedIds = new List<string>();

            // Check which users don't exist
            foreach (var id in userIds)
            {
                if (!users.Any(u => u.Id == id))
                {
                    failedIds.Add(id);
                }
            }

            // Get the valid IDs to delete
            var validIdsToDelete = userIds.Except(failedIds).ToList();

            int initialCount = users.Count;
            users.RemoveAll(u => validIdsToDelete.Contains(u.Id));

            int successCount = initialCount - users.Count;

            await _daprClient.SaveStateAsync(STORE_NAME, USERS_KEY, users);

            return (successCount, failedIds);
        }
        public async Task<User?> UpdateUserAsync(string userId, UserUpdateDto userUpdateDto)
        {
            var users = await GetAllUsers();
            var userToUpdate = users.FirstOrDefault(u => u.Id == userId);

            if (userToUpdate == null)
            {
                return null; // Or throw NotFoundException
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.Email))
            {
                // Optional: Check if email is already taken by another user
                if (users.Any(u => u.Email.Equals(userUpdateDto.Email, StringComparison.OrdinalIgnoreCase) && u.Id != userId))
                {
                    throw new InvalidOperationException($"Email '{userUpdateDto.Email}' is already taken.");
                }
                userToUpdate.Email = userUpdateDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.FullName))
            {
                userToUpdate.FullName = userUpdateDto.FullName;
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.Password))
            {
                userToUpdate.Password = HashPassword(userUpdateDto.Password); // Ensure to hash the new password
            }
            
            userToUpdate.Update(userToUpdate.Username); // Update the UpdatedDate and UpdatedBy fields

            await SaveUser(userToUpdate);
            return userToUpdate;
        }
    }
}