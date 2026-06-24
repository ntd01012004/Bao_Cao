using InventoryService.Messaging;
using InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<InventoryStore>();
builder.Services.AddHostedService<OrderCreatedConsumerHostedService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapGet("/", () => "InventoryService is running. Consumes OrderCreated events from RabbitMQ.");

app.Run();
