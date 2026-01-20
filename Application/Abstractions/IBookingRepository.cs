using System;
using Domain.Entities;

namespace Application.Abstractions;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken ct);
    Task<Booking?> GetAsync(Guid bookingId, CancellationToken ct);
    Task<(IReadOnlyList<Booking> bookings, int totalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct);
}
