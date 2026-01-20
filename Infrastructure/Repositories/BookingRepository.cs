using Application.Abstractions;
using Domain.Entities;
using Marten;

namespace Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly IDocumentSession _session;
    public BookingRepository(IDocumentSession session)
    {
        _session = session;
    }
    public async Task AddAsync(Booking booking, CancellationToken ct)
    {
        _session.Store(booking);
        // Store event for event sourcing
        // var bookingCreatedEvent = new BookingCreatedEvent(
        //     booking.Id,
        //     booking.GuestName,
        //     booking.CreatedAt
        // );
        // _session.Events.StartStream<Booking>(booking.Id, bookingCreatedEvent);

        await _session.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken ct)
    {
        return await _session
            .Query<Booking>()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Booking?> GetAsync(Guid bookingId, CancellationToken ct)
    {
        return await _session.LoadAsync<Booking>(bookingId, ct);
    }

    public async Task<(IReadOnlyList<Booking> bookings, int totalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = _session.Query<Booking>()
            .OrderByDescending(b => b.CreatedAt);

        var totalCount = await query.CountAsync(ct);

        var bookings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (bookings, totalCount);
    }
}
