using MediatR;
using OMS.Core.Queries;

public class GetCartQuery : IRequest<CartResponseDto> {
    public string userId { get; set; }
    public GetCartQuery(string userId)
    {
        this.userId = userId;
    }
}