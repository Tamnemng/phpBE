using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;

public class SetValueCommand : IRequest<Unit>
{
    public string Key { get; set; }
    public string Value { get; set; }

    public SetValueCommand(string key, string value)
    {
        Key = key;
        Value = value;
    }
}