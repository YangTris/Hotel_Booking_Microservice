using MediatR;

namespace Application.Bookings.Queries.GetAllBookings;

public record GetAllBookingsQuery : IRequest<List<BookingListItem>>;

public record BookingListItem(
    Guid Id,
    string GuestName,
    string GuestEmail,
    string RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string Status,
    decimal TotalAmount
);
