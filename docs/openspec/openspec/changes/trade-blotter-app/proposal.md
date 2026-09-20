# Proposal

## Why

Traders require immediate visibility into trade executions and real-time derived positions to monitor market risk and exposure. Build a full-stack Trade Blotter application to enable trade entry, live trade history view, and dynamic calculation of net positions and average execution costs without persisting derived positions.

## What Changes

- Introduce **Backend API Tier** using .NET 8 Web API:
  - `POST /trades`: Endpoint to receive, validate, and enqueue trade execution records into a non-blocking concurrent queue.
  - `GET /trades`: Endpoint to return all persisted trade execution records in reverse chronological order (newest first).
  - `GET /positions`: Endpoint to dynamically calculate and return net share quantities and weighted average cost per symbol from trade history, filtering out symbols with net zero positions.
- Introduce **Database Persistence Tier**:
  - Dedicated background queue processor (`BackgroundService` with `System.Threading.Channels.Channel<Trade>` / `ConcurrentQueue<Trade>`) responsible for reading from the queue and writing trades asynchronously to SQLite DB.
  - Decouples API HTTP response latencies from database file locking and disk write operations.
- Introduce **Frontend UI Tier** using Vue 3 (Composition API), Vite, and Pinia:
  - **Trade Entry Form**: Intuitive input form with validation for symbol, side (Buy/Sell), positive quantity, and positive price.
  - **Trade Blotter Table**: Real-time table displaying trade history with visual indicators for Buy vs. Sell, notional value calculations (`Quantity * Price`), timestamp formatting, and sorting.
  - **Positions Panel**: Live reactive view displaying net position and average cost per active symbol, automatically re-calculated when trades are entered.

## Capabilities

### New Capabilities
- `backend-api`: REST API tier for trade submission, history retrieval, and dynamic position derivation.
- `database-tier`: Database persistence tier using a concurrent queue and background worker to decouple database writes and prevent I/O locking on the API thread.
- `frontend-ui`: Vue 3 single-page application with Pinia state management, trade submission form validation, blotter grid visualization, and reactive positions display.

### Modified Capabilities
- None

## Impact

- **Backend & Database**: New .NET 8 Web API project under `src/TradeBlotter.Api` featuring background queue worker for asynchronous SQLite persistence, along with unit test suite under `src/TradeBlotter.Tests`.
- **Frontend**: New Vue 3 + Vite + Pinia application under `src/TradeBlotter.Web`.
- **Dependencies**: .NET 8 SDK, SQLite EF Core / Microsoft.Data.Sqlite, System.Threading.Channels, Node.js 18+, Vue 3, Pinia, Vite.

