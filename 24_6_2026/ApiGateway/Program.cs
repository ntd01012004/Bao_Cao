using ApiGateway.Services;
using ProductCatalog.Contracts;
using ProductGrpcClient = ProductCatalog.Contracts.ProductCatalog.ProductCatalogClient;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Microservices Demo API Gateway",
        Version = "v1",
        Description = "Gateway tổng hợp: Product (gRPC), Order & Inventory (HTTP), Order publish event qua RabbitMQ."
    });
});

var productServiceUrl = builder.Configuration["Services:ProductService"]
    ?? throw new InvalidOperationException("Services:ProductService is required.");

builder.Services.AddGrpcClient<ProductGrpcClient>(options =>
{
    options.Address = new Uri(productServiceUrl);
});

builder.Services.AddHttpClient<IOrderServiceClient, OrderServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:OrderService"]
        ?? throw new InvalidOperationException("Services:OrderService is required."));
});

builder.Services.AddHttpClient<IInventoryServiceClient, InventoryServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:InventoryService"]
        ?? throw new InvalidOperationException("Services:InventoryService is required."));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Microservices Demo API Gateway v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
