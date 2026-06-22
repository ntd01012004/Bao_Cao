using CleanArchitectureDemo.Application.Interfaces;
using CleanArchitectureDemo.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ✅ MEDIATR 14 - DI THỦ CÔNG (KHÔNG EXTENSIONS PACKAGE)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CleanArchitectureDemo.Application.DTOs.UserDto).Assembly);
});


// DI Repository
builder.Services.AddSingleton<IUserRepository, UserRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();