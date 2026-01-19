namespace Domain.Events;

public record BookingCreatedEvent(
    Guid BookingId,
    string GuestName,
    string GuestEmail,
    string RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int NumberOfGuests,
    decimal TotalAmount,
    DateTime CreatedAt
);
