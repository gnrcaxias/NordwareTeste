using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Infrastructure.Persistence;
using Nordware.Infrastructure.Repositories;

namespace ECommerce.Reservations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("Nordware"));

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
