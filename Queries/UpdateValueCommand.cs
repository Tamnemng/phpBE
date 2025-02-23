using MediatR;

public class UpdateValueCommand : IRequest<Unit>
{
    public string Key { get; }
    public string NewValue { get; }

    public UpdateValueCommand(string key, string newValue)
    {
        Key = key;
        NewValue = newValue;
    }
}
