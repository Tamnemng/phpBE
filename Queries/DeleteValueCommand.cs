using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

public class DeleteValueCommand : IRequest<Unit>
{
    public string Key { get; set; }

    public DeleteValueCommand(string key)
    {
        Key = key;
    }
}