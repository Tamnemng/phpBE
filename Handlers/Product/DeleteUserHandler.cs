using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand, bool>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string USERS_KEY = "users";

    public DeleteUserHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new ArgumentException("User ID cannot be empty", nameof(request.UserId));
        }

        var users = await _daprClient.GetStateAsync<List<User>>(
            STORE_NAME,
            USERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<User>();

        // Find the user to delete
        var user = users.FirstOrDefault(u => u.Id == request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID '{request.UserId}' not found");
        }

        // Check if trying to delete an admin account
        if (user.Role == UserRole.Admin)
        {
            // Only allow if there are other admin accounts
            int adminCount = users.Count(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
            {
                throw new InvalidOperationException("Cannot delete the last admin account");
            }
        }

        // Check if trying to delete self
        if (user.Username == request.RequestedBy)
        {
            throw new InvalidOperationException("Administrators cannot delete their own accounts");
        }

        // Remove the user
        int initialCount = users.Count;
        users.RemoveAll(u => u.Id == request.UserId);

        if (users.Count == initialCount)
        {
            return false;
        }

        // Save the updated list
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            USERS_KEY,
            users,
            cancellationToken: cancellationToken
        );

        return true;
    }
}

public class DeleteUsersHandler : IRequestHandler<DeleteUsersCommand, DeleteUsersResult>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string USERS_KEY = "users";

    public DeleteUsersHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<DeleteUsersResult> Handle(DeleteUsersCommand request, CancellationToken cancellationToken)
    {
        if (request.UserIds == null || !request.UserIds.Any())
        {
            throw new ArgumentException("User IDs list cannot be empty", nameof(request.UserIds));
        }

        var result = new DeleteUsersResult();
        
        var users = await _daprClient.GetStateAsync<List<User>>(
            STORE_NAME,
            USERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<User>();

        // Check if last admin would be deleted
        int adminCount = users.Count(u => u.Role == UserRole.Admin);
        var adminIdsToDelete = users
            .Where(u => u.Role == UserRole.Admin && request.UserIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToList();

        if (adminIdsToDelete.Count >= adminCount)
        {
            throw new InvalidOperationException("Cannot delete all admin accounts");
        }

        // Get the admin user making the request
        var adminUser = users.FirstOrDefault(u => u.Username == request.RequestedBy);
        if (adminUser == null)
        {
            throw new InvalidOperationException("Admin user not found");
        }

        // Check if admin is trying to delete themselves
        if (request.UserIds.Contains(adminUser.Id))
        {
            result.FailedIds.Add(adminUser.Id);
            result.FailureReasons.Add("Administrators cannot delete their own accounts");
            
            // Remove admin from the deletion list
            request.UserIds.Remove(adminUser.Id);
        }

        int initialCount = users.Count;

        // Process each user ID for deletion
        foreach (var userId in request.UserIds)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                result.FailedIds.Add(userId);
                result.FailureReasons.Add($"User with ID '{userId}' not found");
                continue;
            }
        }

        // Remove the users that can be deleted
        var validUserIdsToDelete = request.UserIds.Except(result.FailedIds).ToList();
        int beforeDeleteCount = users.Count;
        users.RemoveAll(u => validUserIdsToDelete.Contains(u.Id));
        
        result.SuccessCount = beforeDeleteCount - users.Count;

        // Save the updated list
        await _daprClient.SaveStateAsync(
            STORE_NAME,
            USERS_KEY,
            users,
            cancellationToken: cancellationToken
        );

        return result;
    }
}