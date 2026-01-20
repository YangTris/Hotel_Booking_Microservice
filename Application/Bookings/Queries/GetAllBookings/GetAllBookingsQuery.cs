using Application.Common.Models;
using Domain.Entities.Enums;
using MediatR;

namespace Application.Bookings.Queries.GetAllBookings;


public record GetAllBookingsQuery(
    int PageNumber = 1,
    int PageSize = 5
) : IRequest<PagedResult<BookingListItem>>;

public record BookingListItem(
    Guid Id,
    string GuestName,
    string GuestEmail,
    string RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    BookingStatus Status,
    decimal TotalAmount
);
