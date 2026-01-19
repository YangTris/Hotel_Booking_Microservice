namespace Domain.Events;

public record BookingConfirmedEvent(
    Guid BookingId,
    DateTime ConfirmedAt
);
