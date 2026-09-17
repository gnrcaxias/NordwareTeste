using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nordware.Application.Commands;

namespace Nordware.Infrastructure.Services;

public sealed class ReservationExpirationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ReservationExpirationBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_interval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();

                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                await sender.Send(new ExpireReservationsCommand(), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}