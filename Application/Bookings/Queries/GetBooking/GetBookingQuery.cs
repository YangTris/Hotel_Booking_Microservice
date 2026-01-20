using Domain.Entities;
using Domain.Entities.Enums;
using MediatR;

namespace Application.Bookings.Queries.GetBooking;

public record GetBookingQuery(Guid BookingId) : IRequest<BookingResponse?>;

public record BookingResponse(
    Guid Id,
    string GuestName,
    string GuestEmail,
    string GuestPhone,
    string RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int NumberOfGuests,
    decimal TotalAmount,
    BookingStatus Status,
    DateTime CreatedAt,
    string? Notes
);
