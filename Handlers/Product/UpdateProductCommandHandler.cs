// using MediatR;
// using Dapr.Client;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;

// public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, Unit>
// {
//     private readonly DaprClient _daprClient;
//     private const string STORE_NAME = "statestore";
//     private const string KEY = "brands";

//     public UpdateBrandCommandHandler(DaprClient daprClient)
//     {
//         _daprClient = daprClient;
//     }

//     public async Task<Unit> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
//     {
//         if (request == null)
//         {
//             throw new ArgumentNullException(nameof(request));
//         }
//         var brands = await _daprClient.GetStateAsync<List<Brand>>(STORE_NAME, KEY, cancellationToken: cancellationToken)
//             ?? new List<Brand>();
            
//         var existingProduct = brands.FirstOrDefault(p => p.Id == request.Id);
//         if (existingProduct == null)
//         {
//             throw new KeyNotFoundException($"Product với ID '{request.Id}' không tồn tại.");
//         }
//         existingProduct.Update(request);
//         await _daprClient.SaveStateAsync(STORE_NAME, KEY, brands, cancellationToken: cancellationToken);
//         return Unit.Value;
//     }
// }