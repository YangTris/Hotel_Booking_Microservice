# Hotel Booking Flow - Implementation Guide

## Architecture Overview

This implementation follows **Clean Architecture** with **CQRS** pattern using:

- **MediatR** for command/query separation
- **FluentValidation** for request validation
- **Marten** for PostgreSQL document storage and event sourcing
- **Carter** for minimal API endpoints

## Project Structure

```
Domain/
├── Entities/
│   └── Booking.cs                    # Booking entity with status enum
└── Events/
    ├── BookingCreatedEvent.cs        # Domain event when booking is created
    ├── BookingConfirmedEvent.cs      # Domain event when booking is confirmed
    └── BookingCancelledEvent.cs      # Domain event when booking is cancelled

Application/
├── AssemblyReference.cs              # For assembly scanning
├── Common/
│   └── Behaviors/
│       └── ValidationBehavior.cs     # MediatR pipeline for validation
├── Bookings/
│   ├── Commands/
│   │   └── CreateBooking/
│   │       ├── CreateBookingCommand.cs
│   │       ├── CreateBookingHandler.cs
│   │       └── CreateBookingValidator.cs
│   └── Queries/
│       ├── GetBooking/
│       │   ├── GetBookingQuery.cs
│       │   └── GetBookingHandler.cs
│       └── GetAllBookings/
│           ├── GetAllBookingsQuery.cs
│           └── GetAllBookingsHandler.cs

API/
├── Modules/
│   └── BookingModule.cs              # Carter module with booking endpoints
└── Program.cs                        # Application startup with DI configuration
```

## Booking Flow

### 1. Create Booking (POST /api/bookings)

**Request Flow:**

```
Client → Carter Module → MediatR → ValidationBehavior → CreateBookingHandler → Marten → PostgreSQL
```

**Steps:**

1. Client sends POST request with booking details
2. Carter module receives request and creates `CreateBookingCommand`
3. MediatR dispatches command through pipeline
4. `ValidationBehavior` runs `CreateBookingValidator` rules
5. If valid, `CreateBookingHandler` executes:
   - Creates `Booking` entity with `Pending` status
   - Stores booking in Marten document store
   - Creates `BookingCreatedEvent` for event sourcing
   - Saves to PostgreSQL
6. Returns `CreateBookingResult` with booking ID

**Validation Rules:**

- Guest name: Required, max 100 characters
- Guest email: Required, valid email format
- Guest phone: Required, valid phone format
- Room ID: Required
- Check-in date: Must be today or future
- Check-out date: Must be after check-in
- Number of guests: Between 1 and 10
- Total amount: Must be greater than 0
- Notes: Optional, max 500 characters

### 2. Get Booking by ID (GET /api/bookings/{id})

**Request Flow:**

```
Client → Carter Module → MediatR → GetBookingHandler → Marten → PostgreSQL
```

**Steps:**

1. Client sends GET request with booking ID
2. Carter module creates `GetBookingQuery`
3. `GetBookingHandler` loads booking from Marten
4. Returns `BookingResponse` or 404 if not found

### 3. Get All Bookings (GET /api/bookings)

**Request Flow:**

```
Client → Carter Module → MediatR → GetAllBookingsHandler → Marten → PostgreSQL
```

**Steps:**

1. Client sends GET request
2. Carter module creates `GetAllBookingsQuery`
3. `GetAllBookingsHandler` queries all bookings from Marten
4. Returns list of `BookingListItem` ordered by creation date (newest first)

## Database Schema

Marten automatically creates tables in the `hotel_booking` schema:

```sql
-- Document table for bookings
mt_doc_booking
  id              uuid PRIMARY KEY
  data            jsonb            -- Full booking JSON
  mt_last_modified timestamp
  mt_version       integer
  mt_dotnet_type   varchar

-- Event store tables
mt_streams        -- Event streams
mt_events         -- Event store
```

## API Endpoints

### Create Booking

```http
POST /api/bookings
Content-Type: application/json

{
  "guestName": "John Doe",
  "guestEmail": "john.doe@example.com",
  "guestPhone": "+1234567890",
  "roomId": "ROOM-101",
  "checkInDate": "2026-02-01",
  "checkOutDate": "2026-02-05",
  "numberOfGuests": 2,
  "totalAmount": 500.00,
  "notes": "Late check-in requested"
}
```

**Response (201 Created):**

```json
{
  "bookingId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "message": "Booking created successfully"
}
```

**Error Response (400 Bad Request):**

```json
{
  "message": "Validation failed",
  "errors": [
    {
      "property": "GuestEmail",
      "error": "Invalid email format"
    }
  ]
}
```

### Get Booking by ID

```http
GET /api/bookings/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Response (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "guestName": "John Doe",
  "guestEmail": "john.doe@example.com",
  "guestPhone": "+1234567890",
  "roomId": "ROOM-101",
  "checkInDate": "2026-02-01",
  "checkOutDate": "2026-02-05",
  "numberOfGuests": 2,
  "totalAmount": 500.0,
  "status": "Pending",
  "createdAt": "2026-01-19T10:30:00Z",
  "notes": "Late check-in requested"
}
```

### Get All Bookings

```http
GET /api/bookings
```

**Response (200 OK):**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "guestName": "John Doe",
    "guestEmail": "john.doe@example.com",
    "roomId": "ROOM-101",
    "checkInDate": "2026-02-01",
    "checkOutDate": "2026-02-05",
    "status": "Pending",
    "totalAmount": 500.0
  }
]
```

## Running the Application

### Prerequisites

1. **PostgreSQL** must be running (Docker or local)
2. **Connection string** configured in `appsettings.json`

### Start PostgreSQL with Docker

```powershell
docker run --name postgres-hotel -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
docker exec -it postgres-hotel psql -U postgres -c "CREATE DATABASE hotel_booking;"
```

### Run the API

```powershell
cd API
dotnet run --launch-profile https

# Or use watch mode
dotnet watch
```

### Access Swagger UI

- HTTPS: https://localhost:7291/swagger
- HTTP: http://localhost:5212/swagger

### Test with API.http

Open `API/API.http` in VS Code and use the REST Client extension to test endpoints.

## Event Sourcing

Each booking operation stores events:

```csharp
// Event stream is created with booking ID
_session.Events.StartStream<Booking>(booking.Id, bookingCreatedEvent);
```

**Query events:**

```sql
SELECT * FROM hotel_booking.mt_events
WHERE stream_id = '3fa85f64-5717-4562-b3fc-2c963f66afa6'
ORDER BY version;
```

## Next Steps

### 1. Add More Commands

- `ConfirmBookingCommand` - Change status to Confirmed
- `CancelBookingCommand` - Change status to Cancelled
- `UpdateBookingCommand` - Modify booking details

### 2. Add Business Rules

- Check room availability before creating booking
- Validate room exists in Inventory Service
- Calculate total amount based on room rate and duration

### 3. Add Integration with Other Services

- **Inventory Service** - Check/reserve room availability
- **Payment Service** - Process payment
- **Notification Service** - Send confirmation email

### 4. Add MassTransit for Messaging

```csharp
// After creating booking, publish event
await _publisher.Publish(new BookingCreatedEvent(...));
```

### 5. Implement Saga Pattern

For distributed transactions across services:

```
BookingCreated → CheckAvailability → ProcessPayment → ConfirmBooking
                     ↓ Failed             ↓ Failed
                CancelBooking        RefundPayment
```

## Troubleshooting

### Build Errors

```powershell
# Clean and rebuild
dotnet clean
dotnet build
```

### Database Connection Issues

```powershell
# Check PostgreSQL is running
docker ps

# Test connection
psql -U postgres -h localhost -p 5432
```

### View Marten Schema

```sql
-- Connect to database
\c hotel_booking

-- List schemas
\dn

-- List tables in hotel_booking schema
\dt hotel_booking.*
```

## Testing

### Manual Testing

Use `API.http` file with VS Code REST Client extension.

### Unit Testing (Future)

```csharp
// Test command handler
var handler = new CreateBookingHandler(mockSession);
var result = await handler.Handle(command, CancellationToken.None);
Assert.NotNull(result.BookingId);
```

### Integration Testing (Future)

```csharp
// Test full API endpoint with test database
var response = await client.PostAsJsonAsync("/api/bookings", request);
Assert.Equal(HttpStatusCode.Created, response.StatusCode);
```
