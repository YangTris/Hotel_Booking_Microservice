namespace Domain.Events;

public record BookingCancelledEvent(
    Guid BookingId,
    string Reason,
    DateTime CancelledAt
);
