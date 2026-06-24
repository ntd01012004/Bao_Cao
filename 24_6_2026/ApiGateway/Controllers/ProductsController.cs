using ApiGateway.Models;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Contracts;
using ProductGrpcClient = ProductCatalog.Contracts.ProductCatalog.ProductCatalogClient;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductGrpcClient productClient) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var reply = await productClient.GetProductsAsync(new EmptyRequest(), cancellationToken: cancellationToken);
        var products = reply.Products.Select(MapProduct).ToList();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productClient.GetProductAsync(
                new ProductByIdRequest { Id = id },
                cancellationToken: cancellationToken);
            return Ok(MapProduct(product));
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return NotFound(ex.Status.Detail);
        }
    }

    private static ProductDto MapProduct(ProductReply product) =>
        new(product.Id, product.Name, product.Price, product.Stock);
}
