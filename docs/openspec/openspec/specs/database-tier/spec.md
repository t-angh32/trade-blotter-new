# Database Persistence Tier

## Purpose

Provides an asynchronous, non-blocking database persistence tier that consumes trade execution records from a thread-safe concurrent queue and persists them to SQLite, eliminating disk write latencies and database file locking on main HTTP API request threads.

## Requirements

### Requirement: Thread-Safe Concurrent Trade Queue
The system SHALL maintain an in-memory, thread-safe concurrent queue (`System.Threading.Channels.Channel<Trade>` or `ConcurrentQueue<Trade>`) for incoming trade submissions. The `POST /trades` endpoint MUST write trades to this queue without blocking the HTTP response thread.

#### Scenario: Non-blocking trade enqueueing
- **WHEN** a client submits a trade to `POST /trades`
- **THEN** the system validates the trade, writes it immediately to the concurrent trade queue without waiting for disk I/O, and returns HTTP 201 Created

### Requirement: Asynchronous Background Database Worker
The system SHALL run a background service (`BackgroundService` / `IHostedService`) that continuously reads trade records from the concurrent trade queue and writes them asynchronously to SQLite database persistence.

#### Scenario: Asynchronous trade persistence from queue
- **WHEN** trades are written to the concurrent trade queue
- **THEN** the background worker dequeues trades and persists them to SQLite DB, ensuring thread safety and preventing database write contention

#### Scenario: Graceful queue shutdown
- **WHEN** the application receives a shutdown signal
- **THEN** the background worker drains and persists all remaining trades in the concurrent queue before completing shutdown
