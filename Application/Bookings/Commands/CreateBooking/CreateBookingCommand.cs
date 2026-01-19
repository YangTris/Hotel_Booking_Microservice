using MediatR;

namespace Application.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    string GuestName,
    string GuestEmail,
    string GuestPhone,
    string RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int NumberOfGuests,
    decimal TotalAmount,
    string? Notes
) : IRequest<CreateBookingResult>;

public record CreateBookingResult(
    Guid BookingId,
    string Status,
    string Message
);
