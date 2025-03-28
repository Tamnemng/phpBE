using MediatR;

namespace OMS.Core.Commands
{
    public abstract class Command : IRequest<CommandResponse>
    {
    }

    public abstract class Command<T> : IRequest<CommandResponse<T>>
    {
    }
}
