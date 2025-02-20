using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

public class GetValueQuery : IRequest<string>
{
    public string Key { get; set; }

    public GetValueQuery(string key)
    {
        Key = key;
    }
}