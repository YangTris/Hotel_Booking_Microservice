using Domain.Entities.Enums;

namespace Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public string GuestName { get; set; } = default!;
        public string GuestEmail { get; set; } = default!;
        public string GuestPhone { get; set; } = default!;
        public string RoomId { get; set; } = default!;
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
    }
}