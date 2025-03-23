using MediatR;
using Dapr.Client;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductCommandHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(STORE_NAME, PRODUCTS_KEY, cancellationToken: cancellationToken)
            ?? new List<Product>();
        var existingProduct = products.FirstOrDefault(p => p.Id == request.Id);
        if (existingProduct == null)
        {
            throw new KeyNotFoundException($"Product với ID '{request.Id}' không tồn tại.");
        }

        // Cập nhật các thông tin sản phẩm
        existingProduct.Name = request.Name;
        existingProduct.CategoryId = request.CategoryId;
        existingProduct.Labels = request.Labels;
        existingProduct.UpdatedDate = DateTime.UtcNow;
        existingProduct.UpdatedBy = request.UpdatedBy;

        // Lưu danh sách sản phẩm đã cập nhật
        await _daprClient.SaveStateAsync(STORE_NAME, PRODUCTS_KEY, products, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
