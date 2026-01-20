using Application.Abstractions;
using Domain.Entities;
using Domain.Entities.Enums;
using MediatR;

namespace Application.Bookings.Commands.CreateBooking;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, CreateBookingResponse>
{
    private readonly IBookingRepository _bookingRepository;
    public CreateBookingHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<CreateBookingResponse> Handle(CreateBookingCommand request, CancellationToken ct)
    {
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

        await _bookingRepository.AddAsync(booking, ct);

        return new CreateBookingResponse(
            booking.Id,
            booking.Status.ToString(),
            "Booking created successfully"
        );
    }
}
