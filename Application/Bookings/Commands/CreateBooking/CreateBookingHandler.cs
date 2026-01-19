using Domain.Entities;
using Domain.Entities.Enums;
using Domain.Events;
using Marten;
using MediatR;

namespace Application.Bookings.Commands.CreateBooking;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
{
    private readonly IDocumentSession _session;

    public CreateBookingHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        // Create booking entity
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            RoomId = request.RoomId,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NumberOfGuests = request.NumberOfGuests,
            TotalAmount = request.TotalAmount,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Notes = request.Notes
        };

        // Store booking in Marten
        _session.Store(booking);

        // Store event for event sourcing
        var bookingCreatedEvent = new BookingCreatedEvent(
            booking.Id,
            booking.GuestName,
            booking.GuestEmail,
            booking.RoomId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.NumberOfGuests,
            booking.TotalAmount,
            booking.CreatedAt
        );

        _session.Events.StartStream<Booking>(booking.Id, bookingCreatedEvent);

        // Save changes
        await _session.SaveChangesAsync(cancellationToken);

        return new CreateBookingResult(
            booking.Id,
            booking.Status.ToString(),
            "Booking created successfully"
        );
    }
}
