using FluentValidation;

namespace Application.Bookings.Queries.GetAllBookings;

public class GetAllBookingsQueryValidator : AbstractValidator<GetAllBookingsQuery>
{
    private const int MaxPageSize = 100;
    private const int MinPageSize = 1;

    public GetAllBookingsQueryValidator()
    {
        // Page number must be at least 1
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1");

        // Page size must be between 1 and 100
        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(MinPageSize)
            .WithMessage($"Page size must be at least {MinPageSize}")
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"Page size must not exceed {MaxPageSize}");
    }
}
