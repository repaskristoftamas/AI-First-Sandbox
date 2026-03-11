# Bookstore API — Project Roadmap

> A living reference for what this project aims to explore, what's already in place, and what comes next.

---

## Current State Summary

The project is a Clean Architecture bookstore API built with .NET 10, CQRS (Mediator), DDD, and the Result pattern. It has two aggregate roots (Author, Book) with full CRUD, FluentValidation, strongly-typed IDs, EF Core with SQL Server, JWT authentication, Docker support, and a CI pipeline. Domain and application layers have solid test coverage using xUnit + Shouldly.

**What works today:**
- Clean Architecture with proper dependency flow
- CQRS commands/queries for Author and Book
- Two-layer validation (domain invariants + FluentValidation)
- Result pattern over exceptions
- JWT authentication (HS256)
- Docker Compose (API + SQL Server)
- GitHub Actions CI (build + test)
- OpenAPI with Scalar UI
- Basic pagination on list endpoints

---

## Topic Breakdown

### 1. Domain-Driven Design

| Aspect | Status | Notes |
|--------|--------|-------|
| Entities with private setters + factory methods | Done | Author, Book |
| Strongly-typed IDs | Done | AuthorId, BookId as record structs |
| Domain validation (invariant guards) | Done | `Validate` methods on entities |
| Aggregate roots | Partial | Author and Book exist but Book references Author by ID only — no navigation property on Author |
| Domain events | Not started | No event dispatching; needed for cross-aggregate communication and eventual consistency |
| Value objects beyond IDs | Not started | Candidates: ISBN (currently a raw string), Money (Price + Currency), DateRange |
| Specifications | Not started | Could replace ad-hoc query logic in handlers |

**Next steps:**
- Add an `Author.Books` navigation property (already noted as TODO)
- Promote ISBN to a proper value object with self-validation
- Consider a `Money` value object if multi-currency ever becomes relevant
- Introduce domain events (e.g., `BookCreatedEvent`, `AuthorDeletedEvent`) for decoupled side effects
- Evaluate the Specification pattern for reusable query predicates

---

### 2. ORM & Database Portability

| Aspect | Status | Notes |
|--------|--------|-------|
| EF Core with SQL Server | Done | SqlServer provider, entity configurations, migrations |
| InMemory provider for tests | Done | Application tests use InMemory |
| PostgreSQL support | Not started | EF Core makes this straightforward — swap provider + connection string |
| Database-agnostic query patterns | Mostly done | Using LINQ, no raw SQL |

**Next steps:**
- Add `Npgsql.EntityFrameworkCore.PostgreSQL` package
- Make the provider selection configurable (e.g., `DatabaseProvider` setting that picks SqlServer or PostgreSQL)
- Add a PostgreSQL service to `docker-compose.yml` for local testing
- Ensure all migrations and configurations work with both providers (watch for SQL Server–specific types like `decimal(10,2)` vs PostgreSQL `numeric`)
- Consider a separate `docker-compose.postgres.yml` profile

---

### 3. OpenAPI Documentation

| Aspect | Status | Notes |
|--------|--------|-------|
| OpenAPI generation | Done | `Microsoft.AspNetCore.OpenApi` |
| Scalar UI | Done | Available at `/scalar` in Development |
| Response type annotations | Partial | Endpoints have `.Produces<>()` but could be more detailed |
| Schema descriptions | Not started | Request/response properties lack descriptions |
| API versioning in docs | Not started | No versioning yet |
| Authentication docs | Not started | JWT bearer not documented in OpenAPI spec |

**Next steps:**
- Add XML comment descriptions to request/response records so they flow into OpenAPI schemas
- Document the JWT bearer security scheme in OpenAPI
- Add example values to request models (ISBN format, date format)
- Add API versioning (URL-based `/api/v1/` or header-based) and reflect it in OpenAPI
- Consider generating a typed API client (via NSwag or Kiota) for the future React UI

---

### 4. CI/CD Pipelines

| Aspect | Status | Notes |
|--------|--------|-------|
| Build + test on PR | Done | `ci.yml` workflow |
| Automated code review | Done | `grumpy-reviewer` workflow |
| Project board automation | Done | PR → Ready for Review |
| Docker image build in CI | Not started | |
| Deployment pipeline | Not started | |
| Environment-specific configs | Not started | Only Development config exists |

**Next steps:**
- Add a CI step to build and push the Docker image to GitHub Container Registry (ghcr.io)
- Create a staging environment (e.g., Azure Container Apps, Railway, or a self-hosted VM)
- Add deployment workflows: staging (auto on merge to main) and production (manual approval)
- Add environment-specific `appsettings.Staging.json` and `appsettings.Production.json`
- Add database migration step in the deployment pipeline
- Consider infrastructure-as-code (Bicep for Azure, Terraform for multi-cloud)

---

### 5. Docker & Containerization

| Aspect | Status | Notes |
|--------|--------|-------|
| Multi-stage Dockerfile | Done | Build → publish → runtime |
| Docker Compose (API + SQL Server) | Done | Local dev setup |
| Health checks | Partial | SQL Server has health check; API does not |
| .dockerignore | Not verified | Should exclude bin/obj/tests |
| Container orchestration | Not started | |

**Next steps:**
- Add ASP.NET Core health check endpoints (`/health`, `/health/ready`)
- Add health check to the API service in `docker-compose.yml`
- Add `.dockerignore` if missing
- Add Docker Compose profiles for different database backends
- When microservices are added, evaluate Docker Compose for local dev and Kubernetes manifests for deployment
- Consider adding a reverse proxy (YARP or nginx) container

---

### 6. HTTP Status Codes

| Aspect | Status | Notes |
|--------|--------|-------|
| 200 OK for queries | Done | |
| 201 Created with Location header | Done | `CreatedAtRoute` on POST |
| 204 No Content for updates/deletes | Done | |
| 400 Bad Request for validation | Done | With per-field errors in Problem Details |
| 404 Not Found | Done | |
| 409 Conflict | Done | ISBN uniqueness, author with books |
| 401/403 for auth | Partial | JWT required on write endpoints; no 403 differentiation |
| 429 Too Many Requests | Not started | Rate limiting not implemented |
| 500 Internal Server Error | Done | Global exception handler |

**Next steps:**
- Implement rate limiting middleware and return 429 with `Retry-After` header
- Return proper 403 when authenticated but unauthorized (role-based)
- Consider 422 Unprocessable Entity as an alternative to 400 for domain validation failures
- Add `ETag` / `304 Not Modified` support for GET endpoints (caching)

---

### 7. Pagination & Rate Limiting

| Aspect | Status | Notes |
|--------|--------|-------|
| Offset-based pagination | Done | `page` + `pageSize` query params, skip/take |
| Pagination metadata in response | Not started | No total count, page count, or next/prev links |
| Cursor-based pagination | Not started | Better for large datasets |
| Rate limiting | Not started | TODO in code |

**Next steps:**
- Return pagination metadata (total count, page count, has next/previous) — either in response body as a wrapper or via `Link` / custom headers
- Add `Microsoft.AspNetCore.RateLimiting` middleware with fixed-window or token-bucket policy
- Apply rate limiting globally with higher limits for authenticated users
- Consider cursor-based pagination for the books endpoint (keyset pagination using `BookId`)
- Extract a reusable `PagedResult<T>` type in SharedKernel

---

### 8. Authentication & Authorization

| Aspect | Status | Notes |
|--------|--------|-------|
| JWT Bearer (HS256) | Done | Token validation configured |
| Token generation endpoint | Not started | No login/register endpoints |
| Refresh tokens | Not started | |
| Role-based authorization | Not started | TODO: Admin role for delete |
| Policy-based authorization | Not started | |
| OAuth 2.0 | Not started | Goal requirement |

**Next steps:**
- **Phase 1 — Complete JWT flow:**
  - Add a User entity/aggregate (email, password hash, roles)
  - Add `/api/auth/register` and `/api/auth/login` endpoints that issue JWTs
  - Add refresh token support (stored in DB, rotated on use)
  - Add role claims and policy-based authorization (`[Authorize(Policy = "AdminOnly")]`)

- **Phase 2 — OAuth 2.0:**
  - Integrate an identity provider (options: Keycloak in Docker, Auth0, or ASP.NET Identity)
  - Support Authorization Code flow for the future React UI
  - Support Client Credentials flow for service-to-service communication
  - Keep simple JWT as a fallback for development/testing

---

### 9. Microservices

| Aspect | Status | Notes |
|--------|--------|-------|
| Current architecture | Monolith | Single API, single DB |
| Service boundaries | Identified | Catalog (books/authors) is one bounded context |

**Meaningful microservice candidates:**

1. **Catalog Service** (current API) — Books, Authors, ISBN management
2. **Identity Service** — User management, authentication, token issuance
3. **Order Service** — Shopping cart, order placement, order history
4. **Notification Service** — Email/push notifications triggered by domain events
5. **Search Service** — Full-text search over books (Elasticsearch/Meilisearch)
6. **AI Service** — Diagram generation, recommendations (see section 11)

**Next steps:**
- Start with the Identity Service as the first extraction — it has a clear bounded context and enables OAuth 2.0
- Define API contracts between services (OpenAPI specs or protobuf)
- Use API Gateway pattern (YARP) for routing and cross-cutting concerns
- Each service gets its own database (database-per-service pattern)
- Communicate via async messaging for non-critical paths (see section 10)

---

### 10. Messaging (RabbitMQ / Kafka / Redis / MassTransit)

| Aspect | Status | Notes |
|--------|--------|-------|
| Message broker | Not started | |
| Event-driven communication | Not started | No domain events yet |

**Meaningful use cases in the bookstore domain:**

| Use Case | Publisher | Consumer | Broker |
|----------|-----------|----------|--------|
| New book added → update search index | Catalog Service | Search Service | RabbitMQ |
| Order placed → send confirmation email | Order Service | Notification Service | RabbitMQ |
| Order placed → update inventory count | Order Service | Catalog Service | RabbitMQ |
| Price changed → notify wishlist users | Catalog Service | Notification Service | RabbitMQ |
| High-throughput analytics events (page views, searches) | All Services | Analytics Service | Kafka |
| Cache invalidation on entity update | Catalog Service | API Gateway / Cache | Redis Pub/Sub |
| Distributed caching (book details, author profiles) | — | — | Redis |

**Next steps:**
- Add RabbitMQ to `docker-compose.yml`
- Integrate MassTransit as the messaging abstraction (supports RabbitMQ, Kafka, and InMemory for tests)
- Start with a simple flow: `BookCreatedEvent` published by Catalog → consumed by a Notification stub
- Add Redis for distributed caching (book detail responses, reducing DB load)
- Kafka only when high-throughput event streaming is needed (analytics, audit log)

---

### 11. AI & Generative UI

| Aspect | Status | Notes |
|--------|--------|-------|
| AI integration | Not started | Goal: generative UI — AI composes the interface from user prompts |
| LLM provider abstraction | Not started | Switchable between local (Ollama) and cloud (Claude API) |
| Generative UI protocol stack | Not started | AG-UI (transport) + json-render component catalog (payload) |
| MCP tool layer | Not started | MCP server wrapping .NET API endpoints as AI-callable tools |

**Core concept — Generative UI:**

Instead of a traditional static frontend with predefined pages, the React UI uses a **generative canvas** where an AI agent composes the interface at runtime based on user prompts (text or voice). The AI selects from a predefined React component catalog and streams layout instructions to the frontend via AG-UI events.

> *"Show me all fantasy books sorted by rating as cards"* → card grid
> *"Which genre has the most books? Show a chart"* → pie chart
> *"Compare prices between genres as a bar chart"* → bar chart
> *"List authors who have more than 3 books"* → filtered author list

The user gets infinite views over the same data — no static pages to navigate, no features to discover through menus.

**4-layer architecture:**

```
Layer 4: UI Payload       →  json-render component catalog (React)
                              AI returns JSON matching the catalog schema;
                              frontend renders progressively as AI streams

Layer 3: Agent ↔ UI       →  AG-UI protocol over SSE/WebSocket
                              16 event types for real-time bidirectional
                              communication between AI agent and frontend

Layer 2: Tools & Context  →  MCP server wrapping .NET API
                              Tools: query_books, get_book, query_authors,
                              get_author, get_stats, search_catalog

Layer 1: Agent ↔ Agent    →  Not needed initially (single agent)
                              Later: Catalog Agent ↔ Recommendation Agent
```

**Switchable LLM provider:**

The AI agent layer abstracts the LLM behind a provider interface, allowing runtime switching between local and cloud inference:

| Provider | Engine | Models | Use Case |
|----------|--------|--------|----------|
| Local | Ollama (Docker container) | Llama 3.3 70B, Mistral Large, Qwen 2.5 72B | Privacy, offline, development, no API costs |
| Cloud | Claude API | Claude Opus, Sonnet | Maximum capability, complex reasoning, production |

- Provider selection via configuration (`LlmProvider` setting: `ollama` or `claude`)
- Both providers implement the same interface (accept MCP tool definitions, return structured JSON tool calls + UI payloads)
- Ollama runs as a Docker Compose service alongside SQL Server and RabbitMQ; GPU passthrough via `nvidia-container-toolkit` when available
- Smaller models (Llama 3.1 8B, Phi-3) for fast development iteration; larger models for production quality

**Component catalog (AI can compose from these):**

| Component | Purpose |
|-----------|---------|
| `BookCard` | Cover, title, author, price, rating |
| `BookDetail` | Full book page with all metadata |
| `AuthorProfile` | Author info + their books |
| `DataTable` | Sortable, filterable table of any entity |
| `BarChart` | Recharts wrapper |
| `PieChart` | Recharts wrapper |
| `LineChart` | Recharts wrapper |
| `TimelineView` | Events/orders over time |
| `KanbanBoard` | Grouped cards (e.g., orders by status) |
| `SearchResults` | Book list with facets |
| `StatCard` | Single metric (total books, avg price) |
| `GridLayout` | Arrange N child components in a grid |
| `EmptyState` | No results or onboarding prompt |

**AI-powered features (built on top of generative UI):**

1. **Data visualization from prompts** — User asks "Show me books published per year" → AI queries data via MCP tools, selects a `BarChart` component, streams it to the canvas
2. **Natural language search** — "Find books about medieval history under $20" → AI calls `search_catalog` tool, renders `SearchResults`
3. **Book recommendations** — "Suggest books similar to X" using embeddings → AI renders `BookCard` grid
4. **Auto-categorization** — Classify books into genres based on title/description
5. **Summary generation** — Generate book descriptions from metadata
6. **Voice control** — Web Speech API or Whisper (local via Ollama) for speech-to-text input; same prompt pipeline as typed text

**Technical approach:**
- Create an AI Service (separate microservice) that hosts the LLM provider abstraction and MCP tool server
- AG-UI event stream (SSE) from AI Service → React frontend for progressive UI rendering
- json-render (Vercel Labs) for guardrailed component rendering — AI can only request components from the predefined catalog
- Queue long-running AI requests via RabbitMQ to handle latency without blocking
- MCP server exposes .NET API endpoints as tools the AI agent can call to fetch/mutate data

---

### 12. Scalability

| Aspect | Status | Notes |
|--------|--------|-------|
| Horizontal scaling readiness | Partial | Stateless API (JWT, no session state), but single DB |
| Vertical scaling | Default | Single instance |

**Vertical scalability improvements:**
- Response caching for read-heavy endpoints (Redis)
- Database query optimization (indexes, compiled queries, projections)
- Connection pooling tuning
- `IAsyncEnumerable` for large result set streaming

**Horizontal scalability improvements:**
- Load balancer in front of multiple API instances (already possible — stateless JWT)
- Read replicas for query endpoints (CQRS makes this natural — commands go to primary, queries to replica)
- Database sharding if data volume demands it (unlikely for a bookstore, but good to explore)
- Kubernetes Horizontal Pod Autoscaler based on CPU/request count
- Distributed caching (Redis) so all instances share cache state

**Next steps:**
- Add Redis caching for GET endpoints as the first scalability win
- Implement EF Core compiled queries for hot paths
- Add response compression middleware
- When deploying, put the API behind a load balancer with 2+ replicas
- Add Kubernetes manifests (Deployment, Service, HPA, Ingress) for container orchestration

---

### 13. React Generative UI Frontend

| Aspect | Status | Notes |
|--------|--------|-------|
| Frontend framework | Not started | React + TypeScript + Vite |
| Static shell | Not started | Sidebar, top bar (prompt input + voice), auth screens |
| Generative canvas | Not started | json-render powered area where AI composes the UI |
| Component catalog | Not started | ~13 components the AI can request (see section 11) |
| AG-UI integration | Not started | SSE event stream from AI Service → React |
| Voice input | Not started | Web Speech API (browser) or Whisper (local via Ollama) |
| Typed API client | Not started | Generated from OpenAPI spec for static shell data needs |

**Architecture:**

```
┌──────────────────────────────────────────────────┐
│  Static Shell (always rendered, not AI-controlled) │
│  ┌────────────┐  ┌─────────────────────────────┐ │
│  │  Sidebar    │  │  TopBar                     │ │
│  │  - nav      │  │  - text prompt input        │ │
│  │  - account  │  │  - voice button (mic)       │ │
│  │  - settings │  │  - user menu                │ │
│  │  - pinned   │  │  - LLM provider toggle      │ │
│  │    views    │  │    (local / cloud)           │ │
│  └────────────┘  └─────────────────────────────┘ │
│  ┌──────────────────────────────────────────────┐ │
│  │  Generative Canvas                            │ │
│  │  - json-render renderer                       │ │
│  │  - AG-UI event listener (SSE)                 │ │
│  │  - progressive rendering as AI streams        │ │
│  │  - user can pin layouts they like → saved     │ │
│  │    as static views in the sidebar             │ │
│  └──────────────────────────────────────────────┘ │
│  ┌──────────────────────────────────────────────┐ │
│  │  Auth screens (Login / Register)              │ │
│  │  - static, standard form-based UI             │ │
│  └──────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────┘
```

**Prerequisites from the backend side:**
- CORS is already configured (currently allows `localhost:3000`)
- OpenAPI spec for typed API client generation (static shell data needs)
- OAuth 2.0 Authorization Code flow for user authentication
- SSE endpoint for AG-UI event streaming from AI Service
- MCP server exposing .NET API endpoints as AI-callable tools

**Interaction flow:**

1. User types or speaks a prompt in the `TopBar`
2. Prompt is sent to the AI Service via SSE (AG-UI protocol)
3. AI agent calls MCP tools to fetch data from the .NET API
4. AI composes a UI layout as JSON matching the component catalog schema
5. json-render progressively renders components in the `GenerativeCanvas` as the AI streams
6. User can interact with rendered components (click, sort, filter) — interactions feed back to the AI as context
7. User can pin a generated layout to the sidebar for quick reuse

---

## Suggested Implementation Phases

### Phase 1 — Harden the Foundation
Complete existing TODOs and close gaps in the current monolith.

- [ ] Add `Author.Books` navigation property
- [ ] Promote ISBN to a value object
- [ ] Add pagination metadata to list responses (`PagedResult<T>`)
- [ ] Implement rate limiting
- [ ] Add health check endpoints
- [ ] Complete OpenAPI documentation (security scheme, descriptions, examples)
- [ ] Add role-based authorization with policies
- [ ] Optimize DeleteAuthor to single query

### Phase 2 — Authentication & Identity
Build out the full auth story.

- [ ] Add User entity and Identity Service (can start as module in the monolith)
- [ ] Register + login endpoints with JWT issuance
- [ ] Refresh token rotation
- [ ] Role management (Admin, User)
- [ ] OAuth 2.0 support via Keycloak (Docker) or ASP.NET Identity

### Phase 3 — Database Portability & Caching
- [ ] Add PostgreSQL provider support (configurable)
- [ ] Add Redis to Docker Compose
- [ ] Implement distributed caching for read endpoints
- [ ] Add EF Core compiled queries for hot paths

### Phase 4 — Messaging & Events
- [ ] Add domain events to entities
- [ ] Add RabbitMQ to Docker Compose
- [ ] Integrate MassTransit
- [ ] Implement first event flow (BookCreated → Notification)
- [ ] Add Redis Pub/Sub for cache invalidation

### Phase 5 — Microservice Extraction
- [ ] Extract Identity Service to its own project/container
- [ ] Add API Gateway (YARP)
- [ ] Define service contracts
- [ ] Database-per-service migration
- [ ] Add Order Service as a new bounded context

### Phase 6 — AI & Generative UI Frontend
Build the AI agent layer and the React generative UI as a unified effort.

**6a — AI Service & LLM Provider Abstraction:**
- [ ] Create AI Service microservice with switchable LLM provider (`ollama` / `claude`)
- [ ] Add Ollama to `docker-compose.yml` (local LLM inference with GPU passthrough)
- [ ] Implement LLM provider interface (tool calling + structured JSON output)
- [ ] Build MCP server wrapping .NET API endpoints as AI-callable tools (`query_books`, `get_book`, `query_authors`, `get_author`, `get_stats`, `search_catalog`)
- [ ] Implement AG-UI event stream (SSE) from AI Service for real-time frontend communication
- [ ] Queue long-running AI requests via RabbitMQ

**6b — React Generative UI:**
- [ ] Scaffold React + TypeScript + Vite project
- [ ] Generate typed API client from OpenAPI (for static shell data needs)
- [ ] Build static shell (sidebar, top bar with prompt input + voice button, auth screens)
- [ ] Build component catalog (~13 React components: `BookCard`, `DataTable`, `PieChart`, `BarChart`, `LineChart`, `KanbanBoard`, `TimelineView`, `SearchResults`, `StatCard`, `GridLayout`, etc.)
- [ ] Integrate json-render for guardrailed progressive rendering from AI-generated JSON
- [ ] Wire AG-UI SSE event listener in the `GenerativeCanvas`
- [ ] Add voice input (Web Speech API for browser; Whisper via Ollama as local alternative)
- [ ] Implement pinnable layouts (user saves generated views for quick reuse)

**6c — AI-Powered Features:**
- [ ] Data visualization from prompts (AI queries data via MCP tools → selects chart component)
- [ ] Natural language search ("Find fantasy books under $15")
- [ ] Book recommendations ("Suggest books similar to X")
- [ ] Auto-categorization and summary generation

### Phase 7 — Deployment & Scalability
- [ ] CI/CD: Docker image build + push to ghcr.io
- [ ] Kubernetes manifests (or Azure Container Apps / similar)
- [ ] Staging + production deployment pipelines
- [ ] Horizontal scaling with load balancer + HPA
- [ ] Read replica configuration for CQRS query side

---

## Quick Reference: Technology Choices

| Concern | Technology | Why |
|---------|-----------|-----|
| API Framework | ASP.NET Core Minimal APIs | Already in use, lightweight |
| CQRS | Mediator (martinothamar) | Already in use, source-generated |
| ORM | EF Core | Already in use, multi-provider |
| Primary DB | SQL Server | Already in use |
| Secondary DB | PostgreSQL | Portability goal |
| Cache | Redis | Industry standard, pub/sub + caching |
| Message Broker | RabbitMQ + MassTransit | MassTransit abstracts broker, easy swap |
| Event Streaming | Kafka (if needed) | High-throughput analytics only |
| Identity | Keycloak (Docker) or ASP.NET Identity | OAuth 2.0 support |
| API Gateway | YARP | .NET-native, high performance |
| AI (local) | Ollama + Llama 3.3 / Mistral / Qwen | Privacy, offline, no API costs |
| AI (cloud) | Claude API (Opus, Sonnet) | Maximum capability, complex reasoning |
| AI tool layer | MCP (Model Context Protocol) | Standard protocol for AI-callable tools |
| Agent ↔ UI transport | AG-UI protocol (SSE) | Industry standard for agent-frontend streaming |
| Generative UI rendering | json-render (Vercel Labs) | Guardrailed component rendering from AI JSON |
| Voice input | Web Speech API / Whisper (Ollama) | Browser-native or local speech-to-text |
| Search | Meilisearch or Elasticsearch | Full-text search for books |
| Containerization | Docker + Kubernetes | Already using Docker |
| CI/CD | GitHub Actions | Already in use |
| Frontend | React + TypeScript + Vite | Goal requirement, json-render ecosystem |
