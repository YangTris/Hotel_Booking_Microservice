# Hotel Booking Microservice - AI Agent Instructions

## Architecture Overview

This is a **Clean Architecture** .NET 9.0 microservice-based hotel booking system with the following services:

### Microservices Structure

- **API Gateway** - YARP reverse proxy for routing and load balancing
- **Booking Service** - Handles room reservations and booking lifecycle
- **Inventory Service** - Manages room availability and hotel catalog
- **Payment Service** - Processes payments and refunds
<!-- - **Notification Service** - Sends email/SMS confirmations -->

### Layer Architecture (per service)

- **API/** - Presentation layer using **Carter** (minimal APIs with module-based routing)
- **Application/** - Business logic, CQRS commands/queries via **MediatR**
- **Domain/** - Core entities, domain events, and business rules
- **Infrastructure/** - Data access via **Marten**, messaging via **MassTransit**

**Key Architectural Decisions**:

- **Carter** for minimal API routing with auto-discovery
- **MediatR** for CQRS pattern (commands/queries separation)
- **Marten** for PostgreSQL document DB with event sourcing support
- **MassTransit** for async messaging between microservices
- **FluentValidation** for request validation pipeline

## Development Workflow

### Building & Running

```powershell
# Build entire solution
dotnet build MySolution.sln

# Run API project
cd API; dotnet run

# Or use launch profiles (see API/Properties/launchSettings.json)
dotnet run --launch-profile https  # Runs on https://localhost:7291
```

The API uses:

- HTTP: `http://localhost:5212`
- HTTPS: `https://localhost:7291`
- Swagger UI available at `/swagger` in Development environment

### Testing API Endpoints

Use `API/API.http` for VS Code REST Client testing:

```http
@API_HostAddress = http://localhost:5212
GET {{API_HostAddress}}/weatherforecast/
```

## Booking Flow Architecture

### Synchronous Flow (Happy Path)

1. **API Gateway** receives booking request → routes to **Booking Service**
2. **Booking Service** validates request via FluentValidation
3. **MediatR** handles `CreateBookingCommand`
4. Command handler checks room availability (call **Inventory Service**)
5. Create booking aggregate in Marten with `BookingCreated` event
6. Publish `BookingCreatedEvent` via MassTransit
7. Return booking confirmation to client

### Asynchronous Flow (Event-Driven)

1. **Inventory Service** consumes `BookingCreatedEvent` → reserves room
2. **Payment Service** consumes `BookingCreatedEvent` → processes payment
3. Payment success → publishes `PaymentCompletedEvent`
4. **Booking Service** consumes `PaymentCompletedEvent` → confirms booking
<!-- 5. **Notification Service** consumes `BookingConfirmedEvent` → sends email/SMS -->

### Saga Pattern for Distributed Transactions

Use MassTransit Saga for compensation if payment fails:

```
BookingCreated → PaymentFailed → ReleaseRoomReservation → CancelBooking
```

## Project-Specific Conventions

### Carter Module Pattern

All API endpoints are defined as Carter modules, NOT controllers:

```csharp
using Carter;
using MediatR;

public class BookingModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/bookings", async (CreateBookingRequest request, ISender sender) =>
        {
            var command = new CreateBookingCommand(request);
            var result = await sender.Send(command);
            return Results.Ok(result);
        })
    }
}
```

**Critical**:

- Each module is auto-discovered by Carter via `app.MapCarter()` in `Program.cs`
- Inject `ISender` (MediatR) for CQRS command/query dispatch
- Modules should be named with `*Module` suffix

### CQRS with MediatR Pattern

**Commands** (write operations):

```csharp
// Application/Bookings/Commands/CreateBooking/CreateBookingCommand.cs
public record CreateBookingCommand(string GuestId, string RoomId, DateOnly CheckIn, DateOnly CheckOut)
    : IRequest<BookingResponse>;

// Handler
public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, BookingResponse>
{
    private readonly IDocumentSession _session;
    private readonly IPublishEndpoint _publisher;

    public async Task<BookingResponse> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        // Business logic, save to Marten, publish event
    }
}
```

**Queries** (read operations):

```csharp
// Application/Bookings/Queries/GetBooking/GetBookingQuery.cs
public record GetBookingQuery(Guid BookingId) : IRequest<BookingResponse>;
```

### FluentValidation Integration

```csharp
// Application/Bookings/Commands/CreateBooking/CreateBookingValidator.cs
public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.CheckIn)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));

        RuleFor(x => x.CheckOut)
            .GreaterThan(x => x.CheckIn);
    }
}
```

Register validation pipeline behavior in `Program.cs`:

```csharp
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

### Marten Configuration Pattern

```csharp
// Infrastructure setup in Program.cs
builder.Services.AddMarten(options =>
{
    options.Connection(builder.Configuration.GetConnectionString("Marten")!);
    options.DatabaseSchemaName = "booking";

    // Event sourcing for booking aggregate
    options.Events.StreamIdentity = StreamIdentity.AsGuid;

    // Document mappings
    options.Schema.For<Booking>().Identity(x => x.Id);
});
```

### MassTransit Configuration Pattern

```csharp
// Infrastructure setup in Program.cs
builder.Services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<BookingCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});
```

### Project Structure Rules

1. **Domain layer**: Entities, value objects, domain events (pure C#, no external deps)
2. **Application layer**: Commands/Queries/Handlers (MediatR), validators (FluentValidation)
3. **Infrastructure layer**: Marten repositories, MassTransit consumers, external APIs
4. **API layer**: Carter modules that dispatch to MediatR handlers

### Tech Stack & Dependencies

**API Layer:**

- **Carter 9.0.0** - Module-based minimal APIs with auto-discovery
- **Swashbuckle.AspNetCore 6.6.2** - Swagger/OpenAPI documentation
- **Microsoft.AspNetCore.OpenApi 9.0.9** - OpenAPI support

**Application Layer:**

- **MediatR** - CQRS pattern implementation (commands/queries)
- **FluentValidation** - Request validation with pipeline behavior
- **MediatR.Extensions.FluentValidation** - Validation integration

**Infrastructure Layer:**

- **Marten** - PostgreSQL document DB with event sourcing
- **MassTransit** - Async messaging (RabbitMQ/Azure Service Bus)
- **MassTransit.RabbitMQ** - RabbitMQ transport

**API Gateway:**

- **Yarp.ReverseProxy** - Microsoft's reverse proxy for .NET

## Configuration & Settings

- **appsettings.json**: Base configuration (logging, allowed hosts)
- **appsettings.Development.json**: Development overrides
- **launchSettings.json**: Defines `http` and `https` profiles for local development

## Implementation Workflow

### Adding a New Feature (Example: Create Booking)

**1. Domain Layer** - Define aggregate and events

```
Domain/Aggregates/Booking.cs
Domain/Events/BookingCreatedEvent.cs
Domain/ValueObjects/DateRange.cs
```

**2. Application Layer** - CQRS commands/queries

```
Application/Bookings/Commands/CreateBooking/
  ├── CreateBookingCommand.cs
  ├── CreateBookingHandler.cs
  └── CreateBookingValidator.cs
```

**3. Infrastructure Layer** - Persistence and messaging

```
Infrastructure/Consumers/BookingCreatedConsumer.cs
Infrastructure/Persistence/BookingRepository.cs (if needed beyond Marten)
```

**4. API Layer** - Expose endpoint

```
API/Modules/BookingModule.cs
```

### Microservice Communication Patterns

**Synchronous (HTTP)**: Use for queries or immediate responses

```csharp
// Call Inventory Service from Booking Service
var response = await _httpClient.GetAsync($"http://inventory-service/api/rooms/{roomId}");
```

**Asynchronous (Events)**: Use for eventual consistency

```csharp
// Publish event after booking created
await _publisher.Publish(new BookingCreatedEvent(bookingId, roomId, guestId));
```

### Domain Event Publishing Pattern

```csharp
// Domain entity raises event
public class Booking : AggregateRoot
{
    public void Create(/*params*/)
    {
        // Business logic
        AddDomainEvent(new BookingCreatedEvent(Id, RoomId, GuestId));
    }
}

// Infrastructure publishes events after save
public async Task SaveAsync(Booking booking)
{
    await _session.StoreAsync(booking);
    await _session.SaveChangesAsync();

    foreach (var domainEvent in booking.DomainEvents)
    {
        await _publisher.Publish(domainEvent);
    }
}
```

## API Gateway with YARP

### Configuration Pattern (appsettings.json)

```json
{
  "ReverseProxy": {
    "Routes": {
      "booking-route": {
        "ClusterId": "booking-cluster",
        "Match": {
          "Path": "/api/bookings/{**catch-all}"
        }
      },
      "inventory-route": {
        "ClusterId": "inventory-cluster",
        "Match": {
          "Path": "/api/rooms/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "booking-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5001"
          }
        }
      },
      "inventory-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "http://localhost:5002"
          }
        }
      }
    }
  }
}
```

### Setup in Program.cs

```csharp
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

app.MapReverseProxy();
```

## Common Gotchas & Best Practices

### Project References

- **API** → Application
- **Application** → Domain
- **Infrastructure** → Application + Domain
- Install NuGet packages in correct layers per tech stack above

### MediatR Handler Registration

```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

### Marten Session Scoping

- Use `IDocumentSession` (scoped) in handlers for unit of work
- Don't inject `IDocumentStore` directly in handlers

### MassTransit Consumer Registration

- Consumers must be registered with `AddConsumer<T>()`
- Use `ConfigureEndpoints()` for automatic queue creation

### Carter Module Discovery

- Modules auto-discovered via `app.MapCarter()`
- Use constructor injection for `ISender`, `IDocumentSession`

### FluentValidation

- Validators auto-discovered if registered with `AddValidatorsFromAssembly()`
- Add pipeline behavior for automatic validation before handler execution

### Nullable Reference Types

- Enabled across all projects (`<Nullable>enable</Nullable>`)
- Be explicit with null handling, use `!` only when certain

### Event Naming Convention

- Past tense: `BookingCreatedEvent`, `PaymentCompletedEvent`
- Place in `Domain/Events/` directory
