using Grpc.Core;
using ProductCatalog.Contracts;

namespace ProductService.Services;

public sealed class ProductGrpcService : ProductCatalog.Contracts.ProductCatalog.ProductCatalogBase
{
    private static readonly IReadOnlyList<ProductReply> Products =
    [
        new ProductReply { Id = 1, Name = "Laptop Dell XPS 13", Price = 1299, Stock = 12 },
        new ProductReply { Id = 2, Name = "Logitech MX Master 3S", Price = 99, Stock = 40 },
        new ProductReply { Id = 3, Name = "Keychron K8 Pro", Price = 109, Stock = 18 }
    ];

    public override Task<ProductListReply> GetProducts(EmptyRequest request, ServerCallContext context)
    {
        var reply = new ProductListReply();
        reply.Products.AddRange(Products);
        return Task.FromResult(reply);
    }

    public override Task<ProductReply> GetProduct(ProductByIdRequest request, ServerCallContext context)
    {
        var product = Products.FirstOrDefault(item => item.Id == request.Id);
        if (product is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} was not found."));
        }

        return Task.FromResult(product);
    }
}
