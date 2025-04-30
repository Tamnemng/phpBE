using MediatR;
using System.Collections.Generic;

public class DeleteUserCommand : IRequest<bool>
{
    public string UserId { get; set; }
    public string RequestedBy { get; set; } // Admin user who requested the deletion

    public DeleteUserCommand(string userId, string requestedBy)
    {
        UserId = userId;
        RequestedBy = requestedBy;
    }
}

public class DeleteUsersCommand : IRequest<DeleteUsersResult>
{
    public List<string> UserIds { get; set; }
    public string RequestedBy { get; set; } // Admin user who requested the deletion

    public DeleteUsersCommand(List<string> userIds, string requestedBy)
    {
        UserIds = userIds;
        RequestedBy = requestedBy;
    }
}

public class DeleteUsersResult
{
    public int SuccessCount { get; set; }
    public List<string> FailedIds { get; set; }
    public List<string> FailureReasons { get; set; }

    public DeleteUsersResult()
    {
        FailedIds = new List<string>();
        FailureReasons = new List<string>();
    }
}
