# Tasks

## 1. Backend Implementation (.NET 8 Web API)

- [ ] 1.1 Scaffold .NET 8 Web API project under `src/TradeBlotter.Api` and xUnit test project under `src/TradeBlotter.Tests`; verify `dotnet build` succeeds.
- [ ] 1.2 Implement `Trade` domain model, `CreateTradeDto`, and SQLite `TradeDbContext` for trade persistence; verify database initialization.
- [ ] 1.3 Implement `PositionCalculatorService` for dynamic net position and weighted average cost derivation (omitting net zero positions); verify calculation behavior.
- [ ] 1.4 Create `TradesController` with `POST /trades`, `GET /trades`, and `GET /positions` endpoints with input validation and CORS policies; verify endpoints via HTTP test/curl.
- [ ] 1.5 Write xUnit unit tests in `src/TradeBlotter.Tests` covering single buys, weighted average costs on mixed trades, and net zero omission; verify `dotnet test` passes clean.

## 2. Frontend Implementation (Vue 3 + Pinia + Vite)

- [ ] 2.1 Scaffold Vue 3 project with Vite and Pinia under `src/TradeBlotter.Web`; verify `npm run dev` boots successfully.
- [ ] 2.2 Implement Pinia store `useTradeStore` for trade submission, trades list fetching, and positions state handling; verify state updates.
- [ ] 2.3 Build `TradeEntryForm.vue` with client-side validation (non-empty symbol, positive quantity/price) and submit logic; verify validation and form clearing.
- [ ] 2.4 Build `TradeBlotter.vue` component displaying reverse-chronological trade history with visual Buy/Sell badges, calculated notional value, and column sorting; verify grid scannability.
- [ ] 2.5 Build `PositionsPanel.vue` component displaying reactive net positions and average cost per symbol; verify reactive updates upon trade entry.

## 3. Integration & Verification

- [ ] 3.1 Verify full-stack integration end-to-end (submitting trade in Vue UI updates backend SQLite, blotter grid, and positions panel immediately).
- [ ] 3.2 Update `README.md` with clear startup instructions for both backend and frontend, design decisions, and future improvements.
