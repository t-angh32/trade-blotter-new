# Backend API Tier

## Purpose

Provides a RESTful C# .NET 8 Web API for receiving trade submissions, fetching trade history, and deriving real-time net positions with average execution cost without persisting position records.

## Requirements

### Requirement: Trade Submission Endpoint
The system SHALL expose a `POST /trades` HTTP endpoint accepting a trade execution payload containing `Symbol`, `Side`, `Quantity`, and `Price`. Upon successful validation, the system MUST immediately insert the trade into the in-memory trade cache (`ITradeCacheService`), enqueue it to the concurrent persistence queue (`Channel<Trade>`), and return an HTTP status code of 201 Created with the trade record.

#### Scenario: Valid trade submission
- **WHEN** a client sends a `POST /trades` request with valid `Symbol` ("AAPL"), `Side` ("Buy"), `Quantity` (100), and `Price` (150.50)
- **THEN** the system inserts the trade into the in-memory trade cache, enqueues it for background DB persistence, and returns HTTP status 201 Created

#### Scenario: Invalid trade submission validation
- **WHEN** a client sends a `POST /trades` request with empty `Symbol`, missing `Side`, or non-positive `Quantity` or `Price`
- **THEN** the system rejects the submission with HTTP status 400 Bad Request and validation error messages

### Requirement: Trade History Retrieval Endpoint via In-Memory Cache
The system SHALL expose a `GET /trades` HTTP endpoint returning current day's trades directly from the in-memory trade cache ordered by `Timestamp` in descending order (newest first), without accessing the database persistence tier on read requests.

#### Scenario: Retrieve trade history from in-memory cache
- **WHEN** a client sends a `GET /trades` request
- **THEN** the system reads current day's trades directly from the in-memory trade cache and returns HTTP status 200 OK with zero database disk I/O

### Requirement: Dynamic Position Calculation Endpoint
The system SHALL expose a `GET /positions` HTTP endpoint that dynamically derives the current net position and weighted average execution cost per symbol from all trades in history. Positions MUST NOT be stored separately in the database.

#### Scenario: Single buy position calculation
- **WHEN** a trade history contains a single trade of Buy 100 AAPL at $150.00
- **THEN** `GET /positions` returns HTTP 200 OK with AAPL net quantity 100 and average cost $150.00

#### Scenario: Mixed buy and sell weighted average cost calculation
- **WHEN** a trade history contains Buy 100 AAPL at $100.00, Buy 100 AAPL at $200.00, and Sell 50 AAPL at $180.00
- **THEN** `GET /positions` returns net quantity 150 AAPL and average cost $150.00 (calculated across buys)

#### Scenario: Zero net position omission
- **WHEN** a trade history contains Buy 100 MSFT at $300.00 and Sell 100 MSFT at $310.00 resulting in net quantity 0
- **THEN** MSFT is completely omitted from the `GET /positions` response array

#### Scenario: Short position calculation
- **WHEN** a trade history contains Sell 100 TSLA at $200.00 without prior buys
- **THEN** `GET /positions` returns TSLA with net quantity -100 and average cost $200.00

### Requirement: Real-Time SignalR Trade Broadcasting Hub
The system SHALL expose an ASP.NET Core SignalR hub endpoint mapped to `/hubs/trades`. Upon successful processing of a trade submission via `POST /trades`, the system MUST immediately broadcast a `TradeExecuted` SignalR message containing the complete created `Trade` object to all active SignalR client connections.

#### Scenario: Broadcast executed trade to connected SignalR clients
- **WHEN** a valid trade is submitted via `POST /trades` from any source (UI client or REST API)
- **THEN** the system broadcasts a `TradeExecuted` event with the created `Trade` payload to all clients connected to `/hubs/trades`

#### Scenario: SignalR Hub CORS and WebSocket negotiation
- **WHEN** a Vue 3 frontend client attempts to connect to `/hubs/trades`
- **THEN** the backend accepts the connection with CORS credentials enabled and establishes a persistent real-time SignalR session
