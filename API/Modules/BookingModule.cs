using Application.Bookings.Commands.CreateBooking;
using Application.Bookings.Queries.GetAllBookings;
using Application.Bookings.Queries.GetBooking;
using Carter;
using MediatR;

namespace API.Modules;

public class BookingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bookings", async (CreateBookingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/bookings/{result.BookingId}", result);
        });

        app.MapGet("/api/bookings/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetBookingQuery(id);
            var booking = await sender.Send(query);

            return booking is not null
                ? Results.Ok(booking)
                : Results.NotFound(new { Message = $"Booking with ID {id} not found" });
        });

        app.MapGet("/api/bookings", async (int pageNumber, int pageSize, ISender sender) =>
        {
            var query = new GetAllBookingsQuery(pageNumber, pageSize);
            var result = await sender.Send(query);

            return Results.Ok(result);
        });
    }
}
