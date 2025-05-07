using MediatR;
using Dapr.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using OMS.Core.Utilities;
using Think4.Services;
using System.ComponentModel;

public class UpdateProductBrandHandler : IRequestHandler<UpdateProductBrandCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private readonly ICloudinaryService _cloudinaryService;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";
    private const string CATEGORIES_KEY = "categories";
    private const string GIFTS_KEY = "gifts";

    public UpdateProductBrandHandler(DaprClient daprClient, ICloudinaryService cloudinaryService)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }

    public async Task<Unit> Handle(UpdateProductBrandCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.ProductCode).ToList();

        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with code '{command.ProductCode}' not found.");
        }

        var brands = await _daprClient.GetStateAsync<List<BrandMetaData>>(
            STORE_NAME,
            BRANDS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<BrandMetaData>();

        if (!brands.Any(b => b.Code == command.BrandCode))
        {
            throw new InvalidOperationException($"Brand with code '{command.BrandCode}' does not exist.");
        }

        foreach (var product in productsToUpdate)
        {
            product.UpdateProductBrand(command.BrandCode, command.UpdatedBy);
        }

        // products = products
        //     .Where(p => !productsToUpdate.Any(ptu => ptu.ProductInfo.Code == p.ProductInfo.Code))
        //     .Concat(productsToUpdate)
        //     .ToList();

        // var categories = await _daprClient.GetStateAsync<List<CategoryMetaData>>(
        //     STORE_NAME,
        //     CATEGORIES_KEY,
        //     cancellationToken: cancellationToken
        // ) ?? new List<CategoryMetaData>();

        // foreach (var categoryCode in command.CategoriesCode)
        // {
        //     if (!categories.Any(c => c.Code == categoryCode))
        //     {
        //         throw new InvalidOperationException($"Category with code '{categoryCode}' does not exist.");
        //     }
        // }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

//update product categories

public class UpdateProductCategoriesHandler : IRequestHandler<UpdateProductCategoriesCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private readonly ICloudinaryService _cloudinaryService;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string BRANDS_KEY = "brands";
    private const string CATEGORIES_KEY = "categories";
    private const string GIFTS_KEY = "gifts";

    public UpdateProductCategoriesHandler(DaprClient daprClient, ICloudinaryService cloudinaryService)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }

    public async Task<Unit> Handle(UpdateProductCategoriesCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.ProductCode).ToList();

        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with code '{command.ProductCode}' not found.");
        }

        var categories = await _daprClient.GetStateAsync<List<CategoryMetaData>>(
            STORE_NAME,
            CATEGORIES_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<CategoryMetaData>();

        foreach (var categoryCode in command.CategoriesCode)
        {
            if (!categories.Any(c => c.Code == categoryCode))
            {
                throw new InvalidOperationException($"Category with code '{categoryCode}' does not exist.");
            }
        }

        foreach (var product in productsToUpdate)
        {
            product.UpdateProductCategory(command.CategoriesCode, command.UpdatedBy);
        }

        // products = products
        //     .Where(p => !productsToUpdate.Any(ptu => ptu.ProductInfo.Code == p.ProductInfo.Code))
        //     .Concat(productsToUpdate)
        //     .ToList();

        // var categories = await _daprClient.GetStateAsync<List<CategoryMetaData>>(
        //     STORE_NAME,
        //     CATEGORIES_KEY,
        //     cancellationToken: cancellationToken
        // ) ?? new List<CategoryMetaData>();

        // foreach (var categoryCode in command.CategoriesCode)
        // {
        //     if (!categories.Any(c => c.Code == categoryCode))
        //     {
        //         throw new InvalidOperationException($"Category with code '{categoryCode}' does not exist.");
        //     }
        // }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductImageHandler : IRequestHandler<UpdateProductImageCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private readonly ICloudinaryService _cloudinaryService;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductImageHandler(DaprClient daprClient, ICloudinaryService cloudinaryService)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }

    public async Task<Unit> Handle(UpdateProductImageCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.ProductCode).ToList();

        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with code '{command.ProductCode}' not found.");
        }

        foreach (var product in productsToUpdate)
        {
            product.UpdateProductImage(command.ImageUrl, command.UpdatedBy);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductStatusByIdHandler : IRequestHandler<UpdateProductStatusByIdCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductStatusByIdHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateProductStatusByIdCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var product = products.FirstOrDefault(p => p.ProductInfo.Id == command.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{command.ProductId}' not found.");
        }

        product.UpdateProductStatus(command.Status, command.UpdatedBy);

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductStatusByCodeHandler : IRequestHandler<UpdateProductStatusByCodeCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductStatusByCodeHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateProductStatusByCodeCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.ProductCode).ToList();

        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with code '{command.ProductCode}' not found.");
        }

        foreach (var product in productsToUpdate)
        {
            product.UpdateProductStatus(command.Status, command.UpdatedBy);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductPriceHandler : IRequestHandler<UpdateVariantPriceCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductPriceHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateVariantPriceCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var product = products.FirstOrDefault(p => p.ProductInfo.Id == command.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{command.ProductId}' not found.");
        }

        product.UpdateProductPrice(command.OriginalPrice, command.CurrentPrice, command.UpdatedBy);

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductNameHandler : IRequestHandler<UpdateProductNameCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductNameHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateProductNameCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.ProductCode).ToList();

        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with code '{command.ProductCode}' not found.");
        }

        foreach (var product in productsToUpdate)
        {
            product.UpdateProductName(command.Name, command.UpdatedBy);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductGiftsHandler : IRequestHandler<UpdateProductGiftsCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";
    private const string GIFTS_KEY = "gifts";

    public UpdateProductGiftsHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<Unit> Handle(UpdateProductGiftsCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var productsToUpdate = products.Where(p => p.ProductInfo.Code == command.ProductCode).ToList();

        if (!productsToUpdate.Any())
        {
            throw new InvalidOperationException($"Product with code '{command.ProductCode}' not found.");
        }

        foreach (var product in productsToUpdate)
        {
            var gifts = await _daprClient.GetStateAsync<List<Gift>>(
                STORE_NAME,
                GIFTS_KEY,
                cancellationToken: cancellationToken
            ) ?? new List<Gift>();

            var giftCodes = command.GiftCodes.Select(g => g.ToLower()).ToList();
            var validGifts = gifts.Where(g => giftCodes.Contains(g.Code.ToLower())).ToList();

            if (!validGifts.Any())
            {
                throw new InvalidOperationException($"No valid gifts found for product with code '{command.ProductCode}'.");
            }

            product.UpdateProductGifts(validGifts, command.UpdatedBy);
        }

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}

public class UpdateProductDescriptionHandler : IRequestHandler<UpdateProductDescriptionsCommand, Unit>
{
    private readonly DaprClient _daprClient;
    private const string STORE_NAME = "statestore";
    private const string PRODUCTS_KEY = "products";

    public UpdateProductDescriptionHandler(DaprClient daprClient)
    {
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }
    public async Task<Unit> Handle(UpdateProductDescriptionsCommand command, CancellationToken cancellationToken)
    {
        var products = await _daprClient.GetStateAsync<List<Product>>(
            STORE_NAME,
            PRODUCTS_KEY,
            cancellationToken: cancellationToken
        ) ?? new List<Product>();

        var product = products.FirstOrDefault(p => p.ProductInfo.Id == command.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID '{command.ProductId}' not found.");
        }

        product.UpdateProductDescription(command.ShortDescription, command.Descriptions, command.UpdatedBy);

        await _daprClient.SaveStateAsync(
            STORE_NAME,
            PRODUCTS_KEY,
            products,
            cancellationToken: cancellationToken
        );

        return Unit.Value;
    }
}
