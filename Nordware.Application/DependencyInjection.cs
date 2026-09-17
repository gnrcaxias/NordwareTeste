using Microsoft.Extensions.DependencyInjection;
using Nordware.Application.Abstractions.Persistence;
using Nordware.Application.Services;

namespace Nordware.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            services.AddScoped<IReservationExpirationService, ReservationExpirationService>();

            return services;
        }
    }
}