using Application.Abstractions;
using MediatR;

namespace Application.Bookings.Queries.GetBooking;

public class GetBookingHandler : IRequestHandler<GetBookingQuery, BookingResponse?>
{
    private readonly IBookingRepository _bookingRepository;
    public GetBookingHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }
    public async Task<BookingResponse?> Handle(GetBookingQuery request, CancellationToken ct)
    {
        var booking = await _bookingRepository.GetAsync(request.BookingId, ct);

        if (booking == null)
        {
            return null;
        }

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
            booking.Status,
            booking.CreatedAt,
            booking.Notes
        );
    }
}
