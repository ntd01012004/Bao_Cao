using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MiniDddCqrsJwt.Application.Abstractions.Data;
using MiniDddCqrsJwt.Application.Abstractions.Security;
using MiniDddCqrsJwt.Infrastructure.Authentication;
using MiniDddCqrsJwt.Infrastructure.Persistence;

namespace MiniDddCqrsJwt.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<JwtTokenValidator>();

        return services;
    }
}
