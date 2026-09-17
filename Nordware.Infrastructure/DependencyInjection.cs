using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Services;
using Nordware.Infrastructure.Persistence;
using Nordware.Infrastructure.Repositories;
using Nordware.Infrastructure.Services;

namespace Nordware.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("Nordware"));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHostedService<ReservationExpirationBackgroundService>();

        return services;
    }
}
