// Handlers/Order/GetUserOrdersHandler.cs
using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Queries;

public class GetUserOrdersHandler : IRequestHandler<GetUserOrdersQuery, PagedModel<OrderSummaryDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string ORDERS_KEY = "orders";

    public GetUserOrdersHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<PagedModel<OrderSummaryDto>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _daprClient.GetStateAsync<List<Order>>(
            STORE_NAME,
            ORDERS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Order>();
        
        // Filter orders for the user
        var userOrders = orders
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedDate)
            .ToList();
        
        var totalCount = userOrders.Count;
        
        if (totalCount == 0)
        {
            return new PagedModel<OrderSummaryDto>(0, new List<OrderSummaryDto>(), request.PageIndex, request.PageSize);
        }
        
        // Apply pagination
        var pagedOrders = userOrders
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(o))
            .ToList();
        
        return new PagedModel<OrderSummaryDto>(totalCount, pagedOrders, request.PageIndex, request.PageSize);
    }
}