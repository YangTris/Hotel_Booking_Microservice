using Application.Bookings.Commands.CreateBooking;
using Application.Bookings.Queries.GetAllBookings;
using Application.Bookings.Queries.GetBooking;
using Carter;
using FluentValidation;
using MediatR;

namespace API.Modules;

public class BookingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bookings")
            .WithTags("Bookings")
            .WithDescription("Hotel booking management endpoints");

        // POST /api/bookings - Create a new booking
        group.MapPost("/", async (CreateBookingRequest request, ISender sender) =>
        {
            try
            {
                var command = new CreateBookingCommand(
                    request.GuestName,
                    request.GuestEmail,
                    request.GuestPhone,
                    request.RoomId,
                    request.CheckInDate,
                    request.CheckOutDate,
                    request.NumberOfGuests,
                    request.TotalAmount,
                    request.Notes
                );

                var result = await sender.Send(command);

                return Results.Created($"/api/bookings/{result.BookingId}", result);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                });

                return Results.BadRequest(new
                {
                    Message = "Validation failed",
                    Errors = errors
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error creating booking",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        })
        .WithName("CreateBooking")
        .WithSummary("Create a new hotel booking")
        .WithDescription("Creates a new booking with guest information and room details")
        .Produces<CreateBookingResult>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/bookings/{id} - Get a booking by ID
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetBookingQuery(id);
            var booking = await sender.Send(query);

            return booking is not null
                ? Results.Ok(booking)
                : Results.NotFound(new { Message = $"Booking with ID {id} not found" });
        })
        .WithName("GetBooking")
        .WithSummary("Get a booking by ID")
        .WithDescription("Retrieves detailed information about a specific booking")
        .Produces<BookingResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // GET /api/bookings - Get all bookings
        group.MapGet("/", async (ISender sender) =>
        {
            var query = new GetAllBookingsQuery();
            var bookings = await sender.Send(query);

            return Results.Ok(bookings);
        })
        .WithName("GetAllBookings")
        .WithSummary("Get all bookings")
        .WithDescription("Retrieves a list of all hotel bookings")
        .Produces<List<BookingListItem>>(StatusCodes.Status200OK);
    }
}

public record CreateBookingRequest(
    string GuestName,
    string GuestEmail,
    string GuestPhone,
    string RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int NumberOfGuests,
    decimal TotalAmount,
    string? Notes
);
