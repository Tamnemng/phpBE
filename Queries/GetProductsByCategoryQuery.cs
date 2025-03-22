using MediatR;

public record GetProductsByCategoryQuery(string CategoryId) : IRequest<List<AddProductCommand>>;