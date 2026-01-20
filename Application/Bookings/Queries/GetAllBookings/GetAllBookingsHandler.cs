using Application.Abstractions;
using Application.Common.Models;
using MediatR;

namespace Application.Bookings.Queries.GetAllBookings;

public class GetAllBookingsHandler : IRequestHandler<GetAllBookingsQuery, PagedResult<BookingListItem>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetAllBookingsHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResult<BookingListItem>> Handle(GetAllBookingsQuery request, CancellationToken ct)
    {
        var (bookings, totalCount) = await _bookingRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            ct
        );

        var items = bookings.Select(b => new BookingListItem(
            b.Id,
            b.GuestName,
            b.GuestEmail,
            b.RoomId,
            b.CheckInDate,
            b.CheckOutDate,
            b.Status,
            b.TotalAmount
        )).ToList();

        return new PagedResult<BookingListItem>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}