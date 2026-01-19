using Marten;
using MediatR;

namespace Application.Bookings.Queries.GetAllBookings;

public class GetAllBookingsHandler : IRequestHandler<GetAllBookingsQuery, List<BookingListItem>>
{
    private readonly IDocumentSession _session;

    public GetAllBookingsHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<List<BookingListItem>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _session
            .Query<Domain.Entities.Booking>()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return bookings.Select(b => new BookingListItem(
            b.Id,
            b.GuestName,
            b.GuestEmail,
            b.RoomId,
            b.CheckInDate,
            b.CheckOutDate,
            b.Status.ToString(),
            b.TotalAmount
        )).ToList();
    }
}
