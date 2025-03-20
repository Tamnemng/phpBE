
using Dapr.Client;
using MediatR;

public class GetLaptopByIdQueryHandler : IRequestHandler<GetLaptopByIdQuery, string>
{
    private readonly DaprClient _daprClient;

    private const string STORE_NAME =  "statestore";

    public GetLaptopByIdQueryHandler(DaprClient daprClient){
        _daprClient = daprClient;
    }

    public async Task<string> Handle(GetLaptopByIdQuery request, CancellationToken cancellationToken){
        return await _daprClient.GetStateAsync<string>(STORE_NAME, request.Id, cancellationToken: cancellationToken);
    }
}