using Microsoft.AspNetCore.Server.Kestrel.Core;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5139, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<ProductGrpcService>();
app.MapGet("/", () => "ProductService is running (gRPC on port 5139). Use ApiGateway Swagger to call product APIs.");

app.Run();
