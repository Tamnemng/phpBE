// Handlers/Order/GetAllOrdersHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Queries;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, PagedModel<OrderSummaryDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string ORDERS_KEY = "orders";
    private const string PRODUCTS_KEY = "products";

    public GetAllOrdersHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<PagedModel<OrderSummaryDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _daprClient.GetStateAsync<List<Order>>(
            STORE_NAME,
            ORDERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Order>();
        
        var allProducts = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();
        
        if (request.StatusFilter.HasValue)
        {
            orders = orders.Where(o => o.Status == request.StatusFilter.Value).ToList();
        }
        
        orders = orders.OrderByDescending(o => o.CreatedDate).ToList();
        
        var totalCount = orders.Count;
        
        if (totalCount == 0)
        {
            return new PagedModel<OrderSummaryDto>(0, new List<OrderSummaryDto>(), request.PageIndex, request.PageSize);
        }
        
        // Apply pagination
        var pagedOrders = orders
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(o, allProducts))
            .ToList();
        
        return new PagedModel<OrderSummaryDto>(totalCount, pagedOrders, request.PageIndex, request.PageSize);
    }
}