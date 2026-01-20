using Application.Abstractions;
using Infrastructure.Repositories;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Marten
        services.AddMarten(options =>
        {
            options.Connection(configuration.GetConnectionString("Marten")!);
            options.DatabaseSchemaName = "hotel_booking";
        }).UseLightweightSessions();

        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
