using Marten;
using MediatR;

namespace Application.Bookings.Queries.GetBooking;

public class GetBookingHandler : IRequestHandler<GetBookingQuery, BookingResponse?>
{
    private readonly IDocumentSession _session;

    public GetBookingHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<BookingResponse?> Handle(GetBookingQuery request, CancellationToken cancellationToken)
    {
        var booking = await _session.LoadAsync<Domain.Entities.Booking>(request.BookingId, cancellationToken);

        if (booking == null)
            return null;

        return new BookingResponse(
            booking.Id,
            booking.GuestName,
            booking.GuestEmail,
            booking.GuestPhone,
            booking.RoomId,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.NumberOfGuests,
            booking.TotalAmount,
            booking.Status.ToString(),
            booking.CreatedAt,
            booking.Notes
        );
    }
}
