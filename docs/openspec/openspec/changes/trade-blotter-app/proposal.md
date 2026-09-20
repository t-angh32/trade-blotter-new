# Proposal

## Why

Traders require immediate visibility into trade executions and real-time derived positions to monitor market risk and exposure. Build a full-stack Trade Blotter application to enable trade entry, live trade history view, and dynamic calculation of net positions and average execution costs without persisting derived positions.

## What Changes

- Introduce **Backend API Tier** using .NET 8 Web API:
  - `POST /trades`: Endpoint to receive, validate, and persist trade execution records.
  - `GET /trades`: Endpoint to return all persisted trade execution records in reverse chronological order (newest first).
  - `GET /positions`: Endpoint to dynamically calculate and return net share quantities and weighted average cost per symbol from trade history, filtering out symbols with net zero positions.
  - Persistence store using SQLite / SQL Server LocalDB.
- Introduce **Frontend UI Tier** using Vue 3 (Composition API), Vite, and Pinia:
  - **Trade Entry Form**: Intuitive input form with validation for symbol, side (Buy/Sell), positive quantity, and positive price.
  - **Trade Blotter Table**: Real-time table displaying trade history with visual indicators for Buy vs. Sell, notional value calculations (`Quantity * Price`), timestamp formatting, and sorting.
  - **Positions Panel**: Live reactive view displaying net position and average cost per active symbol, automatically re-calculated when trades are entered.

## Capabilities

### New Capabilities
- `backend-api`: REST API tier for trade submission, history retrieval, dynamic position derivation, and database persistence.
- `frontend-ui`: Vue 3 single-page application with Pinia state management, trade submission form validation, blotter grid visualization, and reactive positions display.

### Modified Capabilities
- None

## Impact

- **Backend**: New .NET 8 Web API project under `src/TradeBlotter.Api` (or `src/TradeBlotter.Backend`) with SQLite Entity Framework Core or Dapper persistence, along with unit test suite under `src/TradeBlotter.Tests`.
- **Frontend**: New Vue 3 + Vite + Pinia application under `src/TradeBlotter.Web` (or `src/trade-blotter-ui`).
- **Dependencies**: .NET 8 SDK, SQLite EF Core / Microsoft.Data.Sqlite, Node.js 18+, Vue 3, Pinia, Vite.
