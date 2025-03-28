// using MediatR;
// using Dapr.Client;
// using OMS.Core.Queries;
// using System.Text.Json;

// public class GetAllBrandQueryHandler : IRequestHandler<GetAllBrandQuery, PagedModel<Brand>>
// {
//     private readonly DaprClient _daprClient;
//     private const string STORE_NAME = "statestore";
//     private const string KEY = "brands";

//     public GetAllBrandQueryHandler(DaprClient daprClient)
//     {
//         _daprClient = daprClient;
//     }

//     public async Task<PagedModel<Brand>> Handle(GetAllBrandQuery request, CancellationToken cancellationToken)
//     {
//         if (request.PageIndex < 0)
//         {
//             return new PagedModel<Brand>(0, new List<Brand>(), 0, request.PageSize);
//         }
//         var brands = await _daprClient.GetStateAsync<List<Brand>>(STORE_NAME, KEY, cancellationToken: cancellationToken) 
//             ?? new List<Brand>();

//         var totalCount = brands.Count;
//         if (totalCount == 0)
//         {
//             return new PagedModel<Brand>(0, new List<Brand>(), request.PageIndex, request.PageSize);
//         }

//         // Phân trang
//         int startIndex = request.PageIndex * request.PageSize;
//         if (startIndex >= totalCount)
//         {
//             return new PagedModel<Brand>(totalCount, new List<Brand>(), request.PageIndex, request.PageSize);
//         }

//         var pagedBrands = brands.Skip(startIndex).Take(request.PageSize).ToList();

//         return new PagedModel<Brand>(totalCount, pagedBrands, request.PageIndex, request.PageSize);
//     }
// }