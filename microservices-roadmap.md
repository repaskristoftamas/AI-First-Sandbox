# Microservices Architecture Roadmap for Bookstore API

## Context

The Bookstore API is currently a well-structured .NET 10 monolith following Clean Architecture, CQRS, DDD, and the Result pattern. The end-state vision is an online book webshop with user login, shopping carts, payments (Apple Pay, Stripe), and an internal LLM chatbot for owner analytics.

This document is an architectural guide for incrementally decomposing the monolith into **event-driven microservices** — where every significant action produces an event that other services independently consume and react to. The message bus (RabbitMQ + MassTransit) is the backbone of all inter-service communication. Services do not call each other synchronously; instead, they maintain local projections of the data they need, kept up-to-date via events. Synchronous HTTP is reserved exclusively for frontend-to-gateway traffic (user-initiated request/response).

---

## 1. Service Decomposition — Bounded Contexts

### Identified Services

| # | Service | Responsibility | Database | Source |
|---|---------|---------------|----------|--------|
| 1 | **Catalog Service** | Books, Authors, ISBN, product browsing, inventory | `catalog_db` (PostgreSQL) | Extracted from current monolith |
| 2 | **Identity Service** | Registration, login, JWT/refresh tokens, roles, OAuth | `identity_db` (PostgreSQL) | Extracted from current monolith (User entity) |
| 3 | **Order Service** | Shopping carts, order lifecycle, order history | `order_db` (PostgreSQL) | Greenfield |
| 4 | **Payment Service** | Stripe, Apple Pay integration, payment status, refunds | `payment_db` (PostgreSQL) | Greenfield |
| 5 | **Notification Service** | Email (order confirmation, password reset, shipping) | `notification_db` (PostgreSQL) | Greenfield |
| 6 | **AI/Analytics Service** | LLM chatbot, data statistics, reporting dashboards | `ai_db` (PostgreSQL + pgvector) | Greenfield |
| 7 | **API Gateway** | Routing, auth validation, rate limiting, CORS, BFF | Stateless | New (YARP) |

### Complete Event Catalog

Every significant action in the system produces an event. Services never call each other — they subscribe to events and maintain local projections of the data they need.

```
Catalog Service
  Produces:
    BookCreated       (BookId, Title, AuthorId, AuthorName, Isbn, Price, PublicationYear)
    BookUpdated       (BookId, Title, AuthorId, AuthorName, Isbn, Price, PublicationYear)
    BookDeleted       (BookId)
    PriceChanged      (BookId, OldPrice, NewPrice)
    StockChanged      (BookId, OldQuantity, NewQuantity, Reason)
    AuthorCreated     (AuthorId, FirstName, LastName)
    AuthorUpdated     (AuthorId, FirstName, LastName)
    AuthorDeleted     (AuthorId)
  Consumes:
    OrderPlaced       -> decrement inventory
    OrderCancelled    -> restore inventory

Identity Service
  Produces:
    UserRegistered    (UserId, Email, Roles)
    UserUpdated       (UserId, Email, Roles)
    UserDeleted       (UserId)
    PasswordChanged   (UserId)
    UserLockedOut     (UserId, Reason)
  Consumes:
    (none — Identity is a source of truth, not a reactor)

Order Service
  Produces:
    CartItemAdded     (CartId, UserId, BookId, Quantity)
    CartItemRemoved   (CartId, BookId)
    CartCleared       (CartId, UserId)
    OrderPlaced       (OrderId, UserId, Items[], TotalAmount)
    OrderPaid         (OrderId, PaymentId)
    OrderShipped      (OrderId, TrackingNumber)
    OrderDelivered    (OrderId)
    OrderCancelled    (OrderId, Reason)
  Consumes:
    BookCreated       -> maintain local product projection (title, price, stock)
    BookUpdated       -> update local product projection
    PriceChanged      -> update local price cache, flag stale carts
    StockChanged      -> update local availability view
    UserRegistered    -> maintain local user projection (userId, email)
    UserUpdated       -> update local user projection
    PaymentCompleted  -> transition order to Paid
    PaymentFailed     -> transition order to PaymentFailed

Payment Service
  Produces:
    PaymentCompleted  (PaymentId, OrderId, Amount, GatewayReference)
    PaymentFailed     (PaymentId, OrderId, Reason)
    RefundIssued      (RefundId, PaymentId, OrderId, Amount)
  Consumes:
    OrderPlaced       -> initiate payment processing with gateway

Notification Service
  Produces:
    NotificationSent  (NotificationId, UserId, Channel, Type)
    NotificationFailed(NotificationId, UserId, Reason)
  Consumes:
    UserRegistered    -> send welcome email
    PasswordChanged   -> send security alert email
    OrderPlaced       -> send order confirmation email
    PaymentCompleted  -> send payment receipt email
    PaymentFailed     -> send payment failure email
    OrderShipped      -> send shipment tracking email
    OrderCancelled    -> send cancellation confirmation email
    UserRegistered    -> maintain local user projection (userId, email — needed to send emails)
    UserUpdated       -> update local user projection

AI/Analytics Service
  Produces:
    (none — read-only consumer and query responder)
  Consumes:
    BookCreated       -> index in analytics store
    BookUpdated       -> update analytics store
    OrderPlaced       -> track order metrics (revenue, volume, popular books)
    OrderPaid         -> update revenue metrics
    PaymentCompleted  -> track payment metrics
    UserRegistered    -> track user growth metrics
    StockChanged      -> track inventory trends
```

### Architecture Diagram

The message bus is the central nervous system. Services publish events and consume events — they never call each other directly. The only synchronous path is frontend -> gateway -> service.

```
                          [React Frontend]
                               |
                               | HTTPS (only sync path)
                               v
                         [API Gateway]  <-- YARP
                        /    |    |    \
                       v     v    v     v
                [Catalog] [Order] [Identity] [AI Service]
                    |        |       |           |
                    |        |       |           | SSE (AG-UI)
                    v        v       v           v
  ================================================================
  =              [RabbitMQ — Message Bus]                         =
  =         (all inter-service communication)                    =
  ================================================================
                    |        |              |
                    v        v              v
              [Payment]  [Notification]  [Background Workers]
```

---

## 2. Communication Patterns — Event-Driven First

### Core Principle: Async by Default, Sync Only at the Edge

The default communication pattern between services is **asynchronous events via RabbitMQ + MassTransit**. Services never make synchronous HTTP calls to each other. Instead, each service maintains **local projections** (read-only copies) of the data it needs from other services, kept current by consuming their events.

Synchronous HTTP exists only on the **edge** — between the frontend and the API gateway, and between the gateway and the target service for that specific user request.

### What This Means in Practice

**Without event-driven architecture** (the anti-pattern):
```
User adds book to cart:
  Order Service --sync HTTP--> Catalog Service: "What's the price of book X?"
  Catalog Service responds with price
  Order Service creates cart item with that price

Problem: if Catalog is slow or down, Order is also down.
```

**With event-driven architecture** (the goal):
```
Catalog Service publishes BookCreated(BookId, Title, Price, ...) when a book is created.
Order Service consumes BookCreated, stores {BookId, Title, Price} in its local ProductProjection table.
Catalog Service publishes PriceChanged(BookId, OldPrice, NewPrice) when a price changes.
Order Service consumes PriceChanged, updates its local ProductProjection.

User adds book to cart:
  Order Service reads from its OWN local ProductProjection table — no HTTP call needed.
  Order Service creates cart item.

Catalog can be completely offline. Order still works.
```

### The Only Synchronous Paths

| Path | Why it must be sync |
|------|---------------------|
| Frontend -> Gateway -> Catalog Service (browse books) | User needs data to render page |
| Frontend -> Gateway -> Identity Service (login/register) | User needs JWT immediately |
| Frontend -> Gateway -> Order Service (place order, view cart) | User needs confirmation |
| Frontend -> Gateway -> AI Service (chat) | User needs streamed response |
| Payment Service -> Stripe/Apple Pay API (external gateway) | External API requires request/response |

Note: these are all **frontend-to-service** via the gateway, or **service-to-external-API**. There are **zero service-to-service sync calls**.

### How Services Get Data Without Sync Calls

Each service subscribes to events and builds **local projections** — read-only tables in its own database containing the subset of another service's data that it needs.

| Service | Needs data from | Events consumed | Local projection |
|---------|----------------|-----------------|------------------|
| Order Service | Catalog | `BookCreated`, `BookUpdated`, `PriceChanged`, `StockChanged` | `ProductProjection` (BookId, Title, AuthorName, Price, InStock) |
| Order Service | Identity | `UserRegistered`, `UserUpdated` | `UserProjection` (UserId, Email) |
| Notification Service | Identity | `UserRegistered`, `UserUpdated` | `UserProjection` (UserId, Email) — needed to know where to send emails |
| AI/Analytics Service | Catalog, Order, Identity | All major events | `AnalyticsStore` tables (denormalized for reporting) |
| Catalog Service | Order | `OrderPlaced`, `OrderCancelled` | (none — just decrements/restores its own Inventory directly) |

**Projection staleness**: A projection might be a few hundred ms behind the source. This is acceptable — when the Order Service adds a book to a cart, it uses the locally projected price. If the price changed 200ms ago and the projection hasn't caught up, the order uses a slightly stale price. At checkout, the `OrderPlaced` event carries the snapshotted price so the Payment Service charges exactly what was shown.

### The Canonical Order Flow

Every step is event-driven. No service-to-service HTTP calls.

```
1.  User clicks "Place Order"
2.  Frontend --> Gateway --> Order Service: POST /api/v1/orders
3.  Order Service reads local ProductProjection for prices (no HTTP call to Catalog)
4.  Order Service reads local UserProjection for email (no HTTP call to Identity)
5.  Order Service creates order (status: Pending), snapshots prices into OrderItems
6.  Order Service publishes OrderPlaced (contains items with snapshotted prices)
7.  Order Service returns 202 Accepted to frontend
8.  --- everything below is async, via RabbitMQ ---
9.  Payment Service consumes OrderPlaced -> charges Stripe/Apple Pay
10. Payment Service publishes PaymentCompleted (or PaymentFailed)
11. Order Service consumes PaymentCompleted -> updates status to "Paid", publishes OrderPaid
12. Catalog Service consumes OrderPlaced -> decrements inventory, publishes StockChanged
13. Notification Service consumes OrderPlaced -> sends order confirmation email
14. Notification Service consumes PaymentCompleted -> sends payment receipt email
15. AI/Analytics Service consumes OrderPlaced + PaymentCompleted -> updates metrics
16. Frontend gets status update via SignalR push or polling
```

If Payment fails:
```
10b. Payment Service publishes PaymentFailed
11b. Order Service consumes PaymentFailed -> marks order as Failed, publishes OrderCancelled
12b. Catalog Service consumes OrderCancelled -> restores inventory, publishes StockChanged
13b. Notification Service consumes PaymentFailed -> sends failure email
```

### Where SignalR/WebSocket Fits

- **AI chat streaming**: SSE via AG-UI protocol (one-directional, simpler than WebSocket)
- **Order status updates**: Order Service publishes `OrderPaid`/`OrderShipped` events. The Gateway subscribes and pushes to connected frontend clients via SignalR. The frontend never polls — it receives real-time pushes.
- **Cart staleness alerts**: When `PriceChanged` arrives, Order Service checks active carts. If a cart item's price changed, it can push a notification to the frontend via SignalR: "Price of Book X changed from $15 to $12"

---

## 3. Message Bus Deep Dive

### What Is a Message Bus and Why It Matters

A message bus sits between services and delivers messages reliably. Without it, Service A must call Service B directly — if B is down, A fails too. With a bus, A drops a message and moves on. The bus guarantees at-least-once delivery even if B is temporarily unavailable.

The bus provides:
- **Decoupling**: Publishers don't know who consumes their events
- **Resilience**: Messages queue up when consumers are down
- **Scalability**: Add more consumer instances without changing publishers (competing consumers pattern)

### RabbitMQ + MassTransit — How They Work Together

**RabbitMQ** = the message broker server (stores and routes messages via exchanges, queues, bindings)
**MassTransit** = the .NET abstraction library (like EF Core is to SQL)

```
Your Service Code
       |
       v
  MassTransit (C# abstraction — serialization, topology, retry, saga)
       |
       v
  RabbitMQ.Client (AMQP protocol)
       |
       v
  RabbitMQ Server (broker)
       |
       v
  MassTransit (consuming service)
       |
       v
  Your Consumer Code
```

Without MassTransit, you'd write raw AMQP code. MassTransit handles:
- Exchange/queue creation (convention-based topology)
- Serialization/deserialization
- Retry policies and dead letter queues
- Saga state machines
- Transactional outbox with EF Core
- **In-memory transport for testing** (swap RabbitMQ for in-memory in test config)

### Message Types: Commands vs Events

**Events** = something that already happened (past tense). Publisher doesn't care who handles them. Multiple consumers allowed.

```csharp
// In Bookstore.Contracts (shared library)
public record OrderPlaced(Guid OrderId, Guid UserId, decimal TotalAmount, DateTimeOffset PlacedAt);
public record PaymentCompleted(Guid PaymentId, Guid OrderId, decimal Amount, DateTimeOffset CompletedAt);
public record BookPriceChanged(Guid BookId, decimal OldPrice, decimal NewPrice);
```

**Commands** = directed instruction (imperative tense). Exactly one consumer. Sender expects it to be handled.

```csharp
public record ProcessPayment(Guid OrderId, decimal Amount, string Currency, string PaymentMethod);
public record SendOrderConfirmationEmail(Guid OrderId, string UserEmail, string OrderSummary);
```

**Rule of thumb**: Events for 90% of cross-service communication. Commands only for directed work (process this payment, send this email).

### Shared Contracts Library

```
src/shared/
  Bookstore.Contracts/
    Catalog/
      BookCreated.cs
      BookPriceChanged.cs
    Orders/
      OrderPlaced.cs
      OrderPaid.cs
    Payments/
      PaymentCompleted.cs
      PaymentFailed.cs
    Identity/
      UserRegistered.cs
```

Rules:
- Records only, no classes with behavior
- Only primitive types (no domain entities, no EF Core types)
- Additive changes only (add properties with defaults) to avoid breaking consumers
- Version messages when breaking changes are unavoidable (`OrderPlacedV2`)

### Routing: Exchanges, Queues, Bindings

MassTransit creates this automatically, but understanding it matters for debugging:

- **Exchange**: When you publish `OrderPlaced`, it goes to exchange `Bookstore.Contracts.Orders:OrderPlaced`
- **Queues**: Each consumer type gets its own: `payment-service-order-placed`, `notification-service-order-placed`, `catalog-service-order-placed`
- **Fan-out**: One publish reaches all consumers (default for events)
- **Competing consumers**: 3 instances of Payment Service share one queue — RabbitMQ delivers each message to exactly one instance (round-robin horizontal scaling)

### Error Handling: Retry + Dead Letter Queues

```csharp
cfg.ReceiveEndpoint("payment-service-order-placed", e =>
{
    e.UseMessageRetry(r => r.Exponential(
        retryCount: 5,
        minInterval: TimeSpan.FromSeconds(1),
        maxInterval: TimeSpan.FromMinutes(5),
        intervalDelta: TimeSpan.FromSeconds(5)));

    e.ConfigureConsumer<OrderPlacedConsumer>(context);
});
```

After all retries exhausted, the message goes to a dead letter queue (`payment-service-order-placed_error`). Monitor these; fix the bug and replay, or handle manually.

### Saga Pattern — For Multi-Step Workflows

**Option A: Choreography (start here)** — each service reacts to events independently. No coordinator. The flow emerges from the event chain.

```
OrderPlaced --> Payment processes --> PaymentCompleted --> Order marks as paid
                                                       --> Notification sends email
                                                       --> Catalog decrements stock
```

Pros: Simple, no single point of failure.
Cons: Hard to see full flow in one place. Hard to handle complex edge cases.

**Option B: Saga State Machine (when flows exceed 3-4 steps)** — MassTransit's `MassTransitStateMachine` tracks workflow state in a database. Single place to see the entire order lifecycle.

```csharp
public class OrderSaga : MassTransitStateMachine<OrderSagaState>
{
    public OrderSaga()
    {
        InstanceState(x => x.CurrentState);
        Event(() => OrderPlaced, x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentCompleted, x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderPlaced)
                .Publish(ctx => new ProcessPayment(ctx.Message.OrderId, ...))
                .TransitionTo(AwaitingPayment));

        During(AwaitingPayment,
            When(PaymentCompleted)
                .Publish(ctx => new SendOrderConfirmationEmail(...))
                .TransitionTo(Paid)
                .Finalize());
    }
}
```

**Recommendation**: Start with choreography. Switch to sagas when you need compensating actions (e.g., "if payment fails after inventory was decremented, restore stock").

### Idempotency — Critical in Distributed Systems

Messages can be delivered more than once (at-least-once delivery). If `OrderPlaced` is delivered twice, the customer gets charged twice.

Solutions:
1. **Message deduplication**: Store processed `MessageId`s, skip duplicates
2. **Idempotent operations**: `UPDATE orders SET status = 'Paid' WHERE id = @id` is naturally idempotent. `INSERT` with unique constraint catches duplicates.
3. **MassTransit Transactional Outbox**: Stores outgoing messages in the same DB transaction as entity changes. Prevents "charge succeeded but publish failed" scenario.

---

## 4. Background Workers

In an event-driven architecture, much of the real work happens outside of HTTP request/response cycles. Background workers are `IHostedService` / `BackgroundService` implementations that run continuously, consuming events, maintaining projections, and performing housekeeping.

### Workers Per Service

**Catalog Service**
| Worker | Responsibility | Trigger |
|--------|---------------|---------|
| `InventoryDecrementConsumer` | Consumes `OrderPlaced`, decrements stock, publishes `StockChanged` | `OrderPlaced` event |
| `InventoryRestoreConsumer` | Consumes `OrderCancelled`, restores stock, publishes `StockChanged` | `OrderCancelled` event |
| `OutboxDeliveryWorker` | MassTransit transactional outbox daemon — delivers messages written to DB during `SaveChangesAsync` | Continuous polling |

**Identity Service**
| Worker | Responsibility | Trigger |
|--------|---------------|---------|
| `OutboxDeliveryWorker` | MassTransit outbox daemon | Continuous polling |

**Order Service**
| Worker | Responsibility | Trigger |
|--------|---------------|---------|
| `ProductProjectionConsumer` | Consumes `BookCreated`, `BookUpdated`, `PriceChanged`, `StockChanged` — maintains local `ProductProjection` table | Catalog events |
| `UserProjectionConsumer` | Consumes `UserRegistered`, `UserUpdated` — maintains local `UserProjection` table | Identity events |
| `PaymentResultConsumer` | Consumes `PaymentCompleted`/`PaymentFailed` — transitions order status, publishes `OrderPaid`/`OrderCancelled` | Payment events |
| `CartStalenessWorker` | When `PriceChanged` arrives, checks active carts for affected items and marks them as stale (optionally pushes via SignalR) | `PriceChanged` event |
| `AbandonedCartCleanupWorker` | Periodic — deletes carts not modified in N days | Timed (daily) |
| `OutboxDeliveryWorker` | MassTransit outbox daemon | Continuous polling |

**Payment Service**
| Worker | Responsibility | Trigger |
|--------|---------------|---------|
| `PaymentProcessingConsumer` | Consumes `OrderPlaced` — calls external payment gateway (Stripe/Apple Pay), publishes `PaymentCompleted`/`PaymentFailed` | `OrderPlaced` event |
| `RefundProcessingConsumer` | Consumes `RefundRequested` — calls payment gateway for refund, publishes `RefundIssued` | `RefundRequested` event |
| `PaymentWebhookProcessor` | Processes async webhooks from Stripe/Apple Pay (payment confirmation, disputes) | Stripe webhook |
| `OutboxDeliveryWorker` | MassTransit outbox daemon | Continuous polling |

**Notification Service** (entirely worker-based — no WebApi, just a `Worker` project)
| Worker | Responsibility | Trigger |
|--------|---------------|---------|
| `UserProjectionConsumer` | Consumes `UserRegistered`, `UserUpdated` — maintains local user contact info for sending emails | Identity events |
| `WelcomeEmailConsumer` | Consumes `UserRegistered` — sends welcome email | `UserRegistered` event |
| `SecurityAlertConsumer` | Consumes `PasswordChanged` — sends security alert | `PasswordChanged` event |
| `OrderConfirmationConsumer` | Consumes `OrderPlaced` — sends order confirmation | `OrderPlaced` event |
| `PaymentReceiptConsumer` | Consumes `PaymentCompleted` — sends receipt | `PaymentCompleted` event |
| `PaymentFailureConsumer` | Consumes `PaymentFailed` — sends payment failure notice | `PaymentFailed` event |
| `ShipmentNotificationConsumer` | Consumes `OrderShipped` — sends tracking info | `OrderShipped` event |
| `CancellationConsumer` | Consumes `OrderCancelled` — sends cancellation confirmation | `OrderCancelled` event |
| `NotificationRetryWorker` | Periodic — retries failed email deliveries from the notification log | Timed (every 5 min) |
| `OutboxDeliveryWorker` | MassTransit outbox daemon | Continuous polling |

**AI/Analytics Service**
| Worker | Responsibility | Trigger |
|--------|---------------|---------|
| `AnalyticsIngestionConsumer` | Consumes all major events (`OrderPlaced`, `PaymentCompleted`, `BookCreated`, `UserRegistered`, `StockChanged`, etc.) — writes to denormalized analytics tables | All events |
| `AnalyticsAggregationWorker` | Periodic — pre-aggregates metrics (daily revenue, popular books, user growth) for fast dashboard queries | Timed (hourly) |
| `OutboxDeliveryWorker` | MassTransit outbox daemon | Continuous polling |

### Cross-Cutting Workers

| Worker | Where it runs | Responsibility |
|--------|--------------|----------------|
| `OutboxDeliveryWorker` | Every service that publishes events | MassTransit's transactional outbox daemon. Scans the `OutboxMessage` table and delivers unpublished messages to RabbitMQ. Ensures atomicity: entity changes and event publishing succeed or fail together. |
| `DlqMonitorWorker` | Dedicated monitoring service or sidecar | Watches dead letter queues (`*_error`). Alerts (logs, Slack, email) when messages land in DLQ. Provides a dashboard/API to inspect and replay failed messages. |

### Worker Hosting Pattern

MassTransit consumers are registered as part of the bus configuration — they're not standalone `BackgroundService` classes. MassTransit spins up consumers and manages their lifecycle. Timed workers (cart cleanup, retry, aggregation) are standard `BackgroundService` implementations with `PeriodicTimer`.

```csharp
// In Order Service Program.cs — MassTransit registration
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductProjectionConsumer>();
    x.AddConsumer<UserProjectionConsumer>();
    x.AddConsumer<PaymentResultConsumer>();
    x.AddConsumer<CartStalenessConsumer>();

    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();     // Delivers outbox messages
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        cfg.ConfigureEndpoints(context);
    });
});

// Timed worker — standard BackgroundService
builder.Services.AddHostedService<AbandonedCartCleanupWorker>();
```

---

## 5. Data Management

### Database-per-Service

Each service owns its own database. No service directly queries another's DB. This is the hardest rule and the most commonly violated.

| Service | Database | Key Tables |
|---------|----------|------------|
| Catalog | `catalog_db` | Authors, Books, Categories, Inventory |
| Identity | `identity_db` | Users, RefreshTokens, OAuthProviders |
| Order | `order_db` | Carts, CartItems, Orders, OrderItems (denormalized book snapshots) |
| Payment | `payment_db` | Payments, Refunds |
| Notification | `notification_db` | NotificationTemplates, NotificationLog |
| AI | `ai_db` | ChatHistory, AnalyticsCache, Embeddings (pgvector) |

### Cross-Service Queries — Solved by Local Projections

**Problem**: "Show order #123 with book title, author name, and user email" — data lives in 3 databases.

**Solution: Local projections + snapshots (default approach)**

The Order Service already has everything it needs in its own database:
- `OrderItems` contains snapshotted book title, author name, and price at purchase time (written at order creation, sourced from local `ProductProjection`)
- `UserProjection` contains the user's email (kept current by `UserRegistered`/`UserUpdated` events)

No cross-service call is needed. The Order Service answers the query entirely from its own database.

**For admin dashboards**: The AI/Analytics Service consumes events from all services and builds denormalized analytics tables. The admin dashboard queries the AI/Analytics Service — one service, one database, no composition needed.

**Fallback — BFF pattern at the gateway**: If a specific view genuinely cannot be prebuilt via projections (rare), the gateway makes parallel calls to multiple services and assembles the response. This is a last resort, not the default.

### Event Sourcing — When to Use (and When Not To)

**Not for this project.** Your domain events (already implemented as in-process Mediator notifications) are sufficient. They publish to RabbitMQ without changing your persistence model. Event sourcing adds massive complexity (event store, projections, snapshotting) that isn't justified for a bookstore's CRUD-heavy domain. If you need audit trails later, add an event log table — not full event sourcing.

### Eventual Consistency — The Mental Shift

**Monolith (now)**: Create a book -> immediately queryable. One DB, one transaction.
**Microservices**: Place an order -> inventory might not decrement for a few hundred ms. During that window, another user might order the last copy.

**How to handle it pragmatically**:
- Accept eventual consistency for non-critical paths. If two users order the last copy, detect it when inventory goes negative, send a "sorry, out of stock" email. This is how Amazon works.
- Use strong consistency *within* a single service's DB.
- For payment (the one place it matters), the Payment Service makes a synchronous call to the gateway and only publishes `PaymentCompleted` after the charge succeeds.

### Distributed Data Pain Points

1. **No cross-service JOINs** — denormalize or compose at API level
2. **No referential integrity** — Order stores a `BookId` (Guid) but can't enforce FK to Catalog DB. Solution: never hard-delete products, soft-delete/archive instead.
3. **No cascading deletes** — if an author is deleted from Catalog, Order doesn't know or care (it has its own snapshot)
4. **Reporting is hard** — for analytics spanning services, you need a data warehouse or the AI Service querying multiple APIs

---

## 6. API Gateway

### Why YARP

| Aspect | YARP | nginx | Ocelot |
|--------|------|-------|--------|
| Language | C# (.NET) | C/Lua | C# (abandoned) |
| .NET integration | Native — same DI, middleware | External process | Low maintenance |
| Auth | Full ASP.NET Core pipeline | Limited | Rigid |
| SignalR/WebSocket | Native | Requires config | Supported |
| **Verdict** | **Use this** | Reverse proxy in front of YARP in prod | Avoid |

### Authentication Strategy

**Gateway-level (recommended to start)**: Gateway validates JWTs, forwards `X-User-Id` / `X-User-Role` headers to downstream services on the internal Docker network. Downstream services trust the gateway.

**Later**: Migrate to asymmetric JWT (RS256). Identity Service holds private key, all services validate with public key independently.

### Gateway Responsibilities

Move from your current `Program.cs` to the gateway:
- JWT validation
- Rate limiting (10/60s anonymous, 100/60s authenticated)
- CORS
- Request routing to downstream services
- BFF aggregation endpoints for cross-service views

---

## 7. Observability

### Distributed Tracing (OpenTelemetry)

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddMassTransitInstrumentation()
        .AddOtlpExporter());
```

Traces flow to Grafana Tempo or Jaeger. A single `TraceId` follows a request through Gateway -> Order Service -> RabbitMQ -> Payment Service.

### Logging, Metrics, Health

- **Logging**: Serilog -> Seq (dev) / Grafana Loki (prod). All logs include `TraceId`.
- **Metrics**: OpenTelemetry -> Prometheus -> Grafana dashboards (RED metrics per service, RabbitMQ queue depth, DB connection pool)
- **Health checks**: `/health` (liveness) + `/health/ready` (readiness — DB connected? RabbitMQ connected?). ASP.NET Core built-in + NuGet packages for NpgSql, RabbitMQ, Redis.
- **Service discovery**: Docker Compose DNS (dev) / Kubernetes Service DNS (prod). No Consul/Eureka needed.

---

## 8. Pain Points & Tradeoffs — Honest Assessment

### The Hard Parts

| Pain Point | Reality | Mitigation |
|------------|---------|------------|
| **Distributed transactions** | No "rollback payment if email fails." Compensating actions only. | Sagas, idempotent consumers, monitoring DLQs |
| **Network latency** | Event propagation adds ms-level delay to projections | Local projections eliminate runtime HTTP calls; only event delivery adds latency |
| **Data consistency** | No JOINs, no FKs across services. Local projections may be briefly stale. | Event-carried state transfer keeps projections current; snapshot at order time for correctness |
| **Deployment complexity** | 7 images, 7 containers, 6 databases, RabbitMQ, Redis, gateway | Docker Compose profiles, .NET Aspire for local dev |
| **Local dev complexity** | 12+ containers eating RAM | Docker Compose profiles: only run what you're working on |
| **Testing** | Integration tests need real infra | Testcontainers.NET, Pact for contract testing, MassTransit in-memory transport |
| **Debugging** | Request spans 4 services | OpenTelemetry distributed tracing is essential |

### When Microservices Are Overkill

- Solo developer or team of 2-3
- Traffic fits on a single server
- No separate teams needing independent deploys
- Still figuring out domain boundaries (premature decomposition is worse than a monolith)

**For this project**: The monolith-first approach is correct. Extract services incrementally as domain boundaries solidify. The current codebase (CQRS, domain events, Result pattern, Clean Architecture) provides the right seams for clean extraction.

### The "Distributed Monolith" Anti-Pattern

The worst outcome: services so tightly coupled they must deploy together, share a DB, and changing one breaks another. All the complexity, none of the benefits.

**How to avoid it**: Each service owns its DB. Use async messaging as default. Minimize sync service-to-service calls. Each service must be deployable and testable independently.

---

## 9. Migration Strategy — Strangler Fig Pattern

### Extraction Order and Rationale

```
Phase 1: Monolith handles everything. (current state)

Phase 2: Add API Gateway (YARP) in front of monolith.
         [Frontend] -> [Gateway] -> [Monolith]
         No behavior change — just getting the gateway in place.

Phase 3: Extract Identity Service (strangler fig).
         [Gateway] -> /auth/* -> [Identity Service] -> [identity_db]
                   -> /api/*  -> [Monolith]         -> [catalog_db]
         Remove Users table from monolith DB.

Phase 4: Add RabbitMQ + MassTransit to monolith (now Catalog Service).
         Publish BookCreated events to bus. Prove the infrastructure.

Phase 5: Add Order Service (greenfield).
         [Gateway] -> /api/orders/* -> [Order Service] -> [order_db]
         Order Service consumes Catalog events for local product projection.
         Order Service publishes OrderPlaced — no sync calls to Catalog.

Phase 6: Add Payment + Notification services.
         Payment consumes OrderPlaced, publishes PaymentCompleted.
         Notification consumes events, sends emails.

Phase 7: Add AI Service.
         Calls other services via MCP tools. Streams via SSE.
```

**Why Identity first?**
1. Clearest bounded context — minimal overlap with books/authors
2. User entity already exists in the codebase
3. Authentication is cross-cutting — extraction benefits all other services
4. Smallest extraction — no complex relationships with Catalog
5. Unlocks OAuth 2.0, refresh tokens — prerequisites for everything else

### Branch by Abstraction (Transition Technique)

Your `IApplicationDbContext` is already the seam. When extracting Users:

1. Create `IUserService` interface (`GetByEmail`, `Create`, `ValidateCredentials`)
2. First impl: `DbUserService` using `IApplicationDbContext` (no behavior change)
3. Second impl: `HttpUserService` calling Identity Service via HTTP
4. Flip DI registration from `DbUserService` to `HttpUserService`
5. Remove User tables from monolith DB, remove User code from monolith

No big-bang migration — swap implementations behind an interface.

---

## 10. Docker Compose — Multi-Service Topology

### Network Isolation

```
networks:
  frontend:   # Gateway + React dev server only
  backend:    # All services, databases, RabbitMQ, Redis, observability
```

Only the gateway bridges both networks. Even if a container is compromised on the frontend network, it can't reach database containers.

### Service Dependencies and Health Checks

```
Databases + RabbitMQ start first (health checks)
  -> Application services start after their DB is healthy
    -> Gateway starts after application services are healthy
```

### Local Dev Workflow

Docker Compose profiles to run only what you're working on:

```bash
# Only catalog service + its DB + RabbitMQ:
docker compose --profile catalog up -d
# Run catalog service locally with dotnet run, connected to Docker infra
```

Or use **.NET Aspire** for local orchestration with a dashboard showing all services, logs, and traces.

---

## 11. Technology Stack

| Concern | Technology | Rationale |
|---------|-----------|-----------|
| API Framework | ASP.NET Core Minimal APIs (.NET 10) | Already in use |
| CQRS | Mediator (martinothamar) | Already in use, source-generated |
| ORM | EF Core 10 | Already in use; each service gets its own DbContext |
| Database | PostgreSQL 17 | Already supported; standardize on one DB engine |
| Message Broker | RabbitMQ 3.x + MassTransit 8.x | .NET gold standard; sagas, outbox, retry, DLQ, in-memory test transport |
| Caching | Redis 7 | Distributed cache for read-heavy endpoints |
| API Gateway | YARP 2.x | .NET-native, Microsoft first-party |
| Auth | JWT (RS256) | Asymmetric keys; any service validates with public key |
| Tracing | OpenTelemetry -> Grafana Tempo | Industry standard; auto-instruments ASP.NET, EF Core, MassTransit |
| Logging | Serilog -> Seq (dev) / Loki (prod) | Structured JSON with trace correlation |
| Metrics | OpenTelemetry -> Prometheus -> Grafana | RED dashboards per service |
| Contract Testing | Pact (pact-net) | Verifies message contracts between services |
| Integration Testing | Testcontainers.NET | Real PostgreSQL + RabbitMQ in Docker for tests |
| Containers | Docker Compose (dev) -> Kubernetes (prod) | Helm charts for prod deployment |
| AI (local) | Ollama | Docker container, already planned |
| AI (cloud) | Claude API | Already planned |
| Search | Meilisearch | Lighter than Elasticsearch, great relevance |

### What You Do NOT Need

- **Consul/Eureka**: Docker Compose + Kubernetes provide service discovery natively
- **Kafka**: Overkill — RabbitMQ handles bookstore volume easily. Kafka adds ZooKeeper/KRaft complexity for no benefit here.
- **Istio/Linkerd service mesh**: Overkill at this scale
- **Ocelot**: Abandoned. YARP is the modern .NET gateway.

---

## 12. Proposed Solution Structure

```
Bookstore/
  src/
    shared/
      Bookstore.Contracts/              # Message types (records only) shared across services
        Catalog/                         #   BookCreated, PriceChanged, StockChanged, etc.
        Orders/                          #   OrderPlaced, OrderPaid, OrderCancelled, etc.
        Payments/                        #   PaymentCompleted, PaymentFailed, RefundIssued
        Identity/                        #   UserRegistered, UserUpdated, etc.
        Notifications/                   #   NotificationSent, NotificationFailed
      Bookstore.SharedKernel/            # (existing) Result, Error, EntityBase
      Bookstore.ServiceDefaults/         # Shared config: OpenTelemetry, health checks, Serilog, MassTransit

    services/
      Bookstore.Catalog/                 # Current monolith, renamed
        Bookstore.Catalog.Domain/
        Bookstore.Catalog.Application/
          Consumers/                     #   InventoryDecrementConsumer, InventoryRestoreConsumer
        Bookstore.Catalog.Infrastructure/
        Bookstore.Catalog.WebApi/        #   HTTP API (frontend reads) + MassTransit bus host

      Bookstore.Identity/
        Bookstore.Identity.Domain/
        Bookstore.Identity.Application/
        Bookstore.Identity.Infrastructure/
        Bookstore.Identity.WebApi/       #   HTTP API (login/register) + MassTransit bus host

      Bookstore.Orders/
        Bookstore.Orders.Domain/
        Bookstore.Orders.Application/
          Consumers/                     #   ProductProjectionConsumer, UserProjectionConsumer,
                                         #   PaymentResultConsumer, CartStalenessConsumer
          Workers/                       #   AbandonedCartCleanupWorker
        Bookstore.Orders.Infrastructure/
          Projections/                   #   ProductProjection, UserProjection (EF configs)
        Bookstore.Orders.WebApi/         #   HTTP API (cart, orders) + MassTransit bus host

      Bookstore.Payments/
        Bookstore.Payments.Application/
          Consumers/                     #   PaymentProcessingConsumer, RefundProcessingConsumer
        Bookstore.Payments.Infrastructure/
        Bookstore.Payments.WebApi/       #   Stripe webhook endpoint + MassTransit bus host

      Bookstore.Notifications/
        Bookstore.Notifications.Application/
          Consumers/                     #   WelcomeEmailConsumer, OrderConfirmationConsumer,
                                         #   PaymentReceiptConsumer, ShipmentNotificationConsumer, etc.
          Workers/                       #   NotificationRetryWorker
        Bookstore.Notifications.Infrastructure/
          Projections/                   #   UserProjection (email lookup)
        Bookstore.Notifications.Worker/  #   BackgroundService host — no WebApi, purely event-driven

      Bookstore.AI/
        Bookstore.AI.Application/
          Consumers/                     #   AnalyticsIngestionConsumer
          Workers/                       #   AnalyticsAggregationWorker
        Bookstore.AI.Infrastructure/
        Bookstore.AI.WebApi/             #   HTTP API (chat endpoint) + MassTransit bus host

    gateway/
      Bookstore.Gateway/                 # YARP — routing, auth, rate limiting, SignalR hub

    frontend/
      bookstore-ui/                      # React + TypeScript + Vite

  tests/
    Bookstore.Catalog.Tests/
    Bookstore.Identity.Tests/
    Bookstore.Orders.Tests/
    Bookstore.Payments.Tests/
    Bookstore.Notifications.Tests/
    Bookstore.Contracts.Tests/           # Pact contract tests
    Bookstore.Integration.Tests/         # E2E with Testcontainers

  docker-compose.yml
```

Each service maintains its own Clean Architecture internally, following the same patterns: CQRS with Mediator, Result pattern, FluentValidation, domain events. Every service that publishes or consumes events hosts a MassTransit bus (configured in its `WebApi` or `Worker` project). `Bookstore.ServiceDefaults` extracts common config (OpenTelemetry, MassTransit, health checks, Serilog) so boilerplate isn't duplicated — same pattern as .NET Aspire starter templates.

The Notification Service is the only service without a WebApi project — it is purely event-driven, running as a `Worker` (hosted service) that consumes events and sends emails. All other services have both a WebApi (for frontend-facing HTTP) and MassTransit consumers (for inter-service events) running in the same host process.

---

## Key Seam Files in Current Codebase

These are the files where microservice extraction will plug in:

- `src/backend/Bookstore.Infrastructure/Data/BookstoreDbContext.cs` — Domain event dispatch in `SaveChangesAsync` is where MassTransit outbox integration plugs in
- `src/backend/Bookstore.Application/Abstractions/IApplicationDbContext.cs` — The seam for service extraction; `Users` DbSet gets replaced with `IUserService` HTTP client abstraction
- `src/backend/Bookstore.SharedKernel/Abstractions/IDomainEvent.cs` — In-process domain events stay internal per service; `Bookstore.Contracts` messages cross service boundaries (separate concerns)
- `src/backend/Bookstore.WebApi/Program.cs` — Composition root that splits: gateway handles auth/rate-limiting/CORS, services handle their own domain middleware
- `docker-compose.yml` — Starting point for multi-service topology; RabbitMQ, Redis, per-service DBs, and gateway added incrementally
