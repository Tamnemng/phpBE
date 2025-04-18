using MediatR;
using Dapr.Client;
using OMS.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedModel<ProductSummaryDto>>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";

    public GetAllProductsQueryHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<PagedModel<ProductSummaryDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        if (request.PageIndex < 0)
        {
            return new PagedModel<ProductSummaryDto>(0, new List<ProductSummaryDto>(), 0, request.PageSize);
        }
        
        // Lấy danh sách sản phẩm
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME, 
            PRODUCTS_KEY, 
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        // Lấy danh sách thương hiệu
        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME,
            BRANDS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        // Tạo một dictionary để map từ brand code đến brand name
        var brandDict = brands.ToDictionary(b => b.Code, b => b.Name);

        // Xử lý sản phẩm trùng code bằng cách chỉ lấy sản phẩm đầu tiên cho mỗi code
        var uniqueProducts = products
            .GroupBy(p => p.ProductInfo.Code)
            .Select(g => g.First())
            .ToList();
        
        // Tạo danh sách ProductSummaryDto từ danh sách sản phẩm sau khi đã lọc trùng
        var productSummaries = uniqueProducts.Select(p => 
        {
            // Lấy tên thương hiệu từ brand code, nếu không tìm thấy thì để trống
            string brandName = string.Empty;
            if (!string.IsNullOrEmpty(p.ProductInfo.Brand) && brandDict.ContainsKey(p.ProductInfo.Brand))
            {
                brandName = brandDict[p.ProductInfo.Brand];
            }

            return new ProductSummaryDto(p, brandName);
        }).ToList();

        var totalCount = productSummaries.Count;
        if (totalCount == 0)
        {
            return new PagedModel<ProductSummaryDto>(0, new List<ProductSummaryDto>(), request.PageIndex, request.PageSize);
        }

        // Phân trang
        int startIndex = request.PageIndex * request.PageSize;
        if (startIndex >= totalCount)
        {
            return new PagedModel<ProductSummaryDto>(totalCount, new List<ProductSummaryDto>(), request.PageIndex, request.PageSize);
        }

        var pagedProductSummaries = productSummaries.Skip(startIndex).Take(request.PageSize).ToList();

        return new PagedModel<ProductSummaryDto>(totalCount, pagedProductSummaries, request.PageIndex, request.PageSize);
    }
}