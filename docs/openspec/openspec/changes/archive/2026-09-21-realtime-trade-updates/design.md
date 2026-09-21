# Design

## Context

The backend is built with ASP.NET Core .NET 8 Web API, storing trades in `ITradeCacheService` and persisting them via an in-memory queue (`Channel<Trade>`). The frontend is a Vue 3 Pinia single-page application. Currently, HTTP polling or WebSockets are not enabled, leaving clients isolated without live updates when trades are executed from external REST requests or concurrent browser sessions.

See `proposal.md` for overall motivation and capabilities.

## Goals / Non-Goals

**Goals:**
- Provide sub-second real-time push updates of trade executions across all connected browser clients.
- Automatically trigger position summary recalculations in connected clients whenever a new trade occurs.
- Enable graceful automatic reconnection with full state resynchronization when network connections drop.
- Provide visible connection status indicators in the user interface.

**Non-Goals:**
- Peer-to-peer WebRTC or external message broker clusters (e.g. Redis backplane / RabbitMQ) - a single-node ASP.NET Core SignalR hub is sufficient for this application scope.
- User authentication / authorization for SignalR hub groups (anonymous trading blotter domain).

## Technical Decisions

### Decision 1: ASP.NET Core SignalR over Server-Sent Events (SSE) or HTTP Polling
- **Rationale**: ASP.NET Core includes native SignalR support out of the box with zero external framework dependencies. `@microsoft/signalr` provides typed client libraries for JavaScript/TypeScript with built-in WebSocket negotiation, HTTP long-polling fallback, and automatic reconnection rules.
- **Alternatives Considered**: 
  - *HTTP Short Polling*: High server overhead, wasteful request latency, poor performance.
  - *Raw WebSockets*: Requires custom protocol framing, reconnect state logic, and heartbeat handling. SignalR handles all of this natively.

### Decision 2: CORS Credentials Policy for SignalR Negotiation
- **Rationale**: SignalR WebSockets and negotiation protocol require HTTP credentials support during handshakes. The default `AllowAnyOrigin()` CORS policy throws a runtime exception when `AllowCredentials()` is enabled.
- **Solution**: Update backend CORS configuration to use `SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials()`.

### Decision 3: Event-Driven Broadcast Flow via `IHubContext<TradeHub>`
- **Flow**:
  1. `TradesController.CreateTrade` receives `POST /trades`.
  2. Trade is created, cached in `_tradeCache`, and enqueued to `_tradeQueue`.
  3. `_hubContext.Clients.All.SendAsync("TradeExecuted", trade)` broadcasts the event to all connected clients.
  4. Pinia store receives `TradeExecuted` event, prepends trade (with ID deduplication check), and triggers `fetchPositions()`.

### Decision 4: SignalR Hub Connection Lifecycle in Pinia Store
- **Rationale**: Managing the `HubConnection` inside `useTradeStore` ensures the WebSocket lifecycle is tied to centralized application state.
- **Deduplication Strategy**: When a user submits a trade locally via `submitTrade()`, the POST response returns the created trade. The incoming SignalR `TradeExecuted` broadcast will also fire for all clients. The Pinia store deduplicates incoming trades using `trade.id` before adding to `trades.value`.

## Risks / Trade-offs

- **[Risk] CORS wildcard exception with credentials** → **Mitigation**: Use `SetIsOriginAllowed(_ => true)` instead of `AllowAnyOrigin()` when `AllowCredentials()` is enabled.
- **[Risk] Missed trades during brief disconnects** → **Mitigation**: Configure SignalR `withAutomaticReconnect()`. Upon re-establishing a connection (or after reconnect retries), trigger `loadAllData()` to perform a full sync of trades and positions from `/trades` and `/positions`.
