var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Route 1: Hello world
app.MapGet("/", () => "Hello .NET 8 API running!");

// Route 2: trả về danh sách JSON
app.MapGet("/products", () =>
{
    var products = new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1500 },
        new { Id = 2, Name = "Phone", Price = 800 },
        new { Id = 3, Name = "Tablet", Price = 600 }
    };

    return products;
});

app.Run();