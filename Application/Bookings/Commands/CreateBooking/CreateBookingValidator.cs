using FluentValidation;

namespace Application.Bookings.Commands.CreateBooking;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.GuestName)
            .NotEmpty().WithMessage("Guest name is required")
            .MaximumLength(100).WithMessage("Guest name must not exceed 100 characters");

        RuleFor(x => x.GuestEmail)
            .NotEmpty().WithMessage("Guest email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.GuestPhone)
            .NotEmpty().WithMessage("Guest phone is required")
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Room ID is required");

        RuleFor(x => x.CheckInDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Check-in date must be today or in the future");

        RuleFor(x => x.CheckOutDate)
            .GreaterThan(x => x.CheckInDate)
            .WithMessage("Check-out date must be after check-in date");

        RuleFor(x => x.NumberOfGuests)
            .GreaterThan(0).WithMessage("Number of guests must be at least 1")
            .LessThanOrEqualTo(10).WithMessage("Number of guests cannot exceed 10");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than 0");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
    }
}
