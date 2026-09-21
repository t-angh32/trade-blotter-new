# Spec Delta

## ADDED Requirements

### Requirement: Real-Time SignalR Trade Broadcasting Hub
The system SHALL expose an ASP.NET Core SignalR hub endpoint mapped to `/hubs/trades`. Upon successful processing of a trade submission via `POST /trades`, the system MUST immediately broadcast a `TradeExecuted` SignalR message containing the complete created `Trade` object to all active SignalR client connections.

#### Scenario: Broadcast executed trade to connected SignalR clients
- **WHEN** a valid trade is submitted via `POST /trades` from any source (UI client or REST API)
- **THEN** the system broadcasts a `TradeExecuted` event with the created `Trade` payload to all clients connected to `/hubs/trades`

#### Scenario: SignalR Hub CORS and WebSocket negotiation
- **WHEN** a Vue 3 frontend client attempts to connect to `/hubs/trades`
- **THEN** the backend accepts the connection with CORS credentials enabled and establishes a persistent real-time SignalR session
