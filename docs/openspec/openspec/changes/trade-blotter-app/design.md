# Design Considerations: Trade Blotter Application

## Context

See `proposal.md` for motivation and overall scope. The objective is to build a high-quality, production-grade Trade Blotter application within a 3–4 hour scope, demonstrating clean domain modeling, robust API design, scannable UI, and correct position calculations.

---

## Goals / Non-Goals

**Goals:**
- Provide a clean C# .NET 8 Web API exposing `/trades` and `/positions`.
- Provide a responsive Vue 3 (Composition API) SPA built with Vite and Pinia.
- Accurately derive net position quantities and weighted average costs per symbol dynamically without storing positions in the database.
- Ensure high UI scannability (visual badges for Buy/Sell, formatted currency, click-to-sort columns, instant UI reactivity).
- Deliver unit tests covering position calculation logic and average cost edge cases.

**Non-Goals:**
- Real-time WebSockets/SignalR streaming (REST request/response is sufficient and deterministic for single-user trade entry).
- User authentication, multi-tenancy, or live external market data feeds.

---

## Tier 1 Design Considerations: Backend (.NET 8 Web API)

### 1. Data Access, In-Memory Caching & Persistence Layer
- **Decision**: Implement an In-Memory Trade Cache (`ITradeCacheService`) in the Backend API tier and use Entity Framework Core with SQLite (`tradeblotter.db`) for background persistence.
- **In-Memory Cache Architecture**:
  - `ITradeCacheService` maintains current day's trades in memory using a thread-safe concurrent collection (`ConcurrentBag<Trade>` / `ConcurrentQueue<Trade>`).
  - **Startup**: Seeded from database on app startup for today's trades.
  - **Write**: On `POST /trades`, validated trades are immediately added to the cache and enqueued to `Channel<Trade>`.
  - **Read (`GET /trades` & `GET /positions`)**: Served 100% from the in-memory cache with zero database disk I/O hits.
- **Entity Model**:
  ```csharp
  public class Trade
  {
      public int Id { get; set; }
      public string Symbol { get; set; } = string.Empty;
      public TradeSide Side { get; set; } // Enum: Buy, Sell
      public decimal Quantity { get; set; }
      public decimal Price { get; set; }
      public DateTime Timestamp { get; set; } // UTC
  }
  ```


### 2. Dynamic Position Derivation Logic
- **Decision**: Enforce dynamic position derivation inside a dedicated domain service (`PositionCalculatorService`).
- **Short Positions**: Explicitly permitted. If `SELL` trades occur without prior `BUY` trades or exceed open long quantities, net position quantity becomes negative ($\text{NetQty} < 0$).
- **Position Calculation Algorithm**:
  - Iterate trades for each symbol chronologically starting from zero net quantity and zero cost basis.
  - **When Long ($\text{NetQty} \ge 0$)**:
    - **BUY**: Increases position size. Weighted average cost updates:
      $$\text{NewAvgCost} = \frac{(\text{CurrentQty} \times \text{CurrentAvgCost}) + (\text{TradeQty} \times \text{TradePrice})}{\text{CurrentQty} + \text{TradeQty}}$$
    - **SELL**: Decreases position size ($\text{NewQty} = \text{CurrentQty} - \text{TradeQty}$). Unit average cost per share remains unchanged. If selling flips position from long to short ($\text{NewQty} < 0$), the excess sold quantity forms a short position at the trade price.
  - **When Short ($\text{NetQty} < 0$)**:
    - **SELL**: Increases short position size ($|\text{NewQty}| = |\text{CurrentQty}| + \text{TradeQty}$). Weighted average short entry price updates:
      $$\text{NewAvgCost} = \frac{(|\text{CurrentQty}| \times \text{CurrentAvgCost}) + (\text{TradeQty} \times \text{TradePrice})}{|\text{CurrentQty}| + \text{TradeQty}}$$
    - **BUY (Cover)**: Decreases short position size ($\text{NewQty} = \text{CurrentQty} + \text{TradeQty}$). Average short entry price remains unchanged for remaining short shares.
  - **Omission Rule**: Any symbol where $\text{NetQty} == 0$ is excluded from the returned position list.

### 3. API Contract & Validation
- Use FluentValidation or DataAnnotations on `CreateTradeDto`:
  - `Symbol`: Required, non-empty, auto-trimmed to uppercase.
  - `Side`: Must match `Buy` or `Sell` (case-insensitive conversion).
  - `Quantity`: Must be $> 0$.
  - `Price`: Must be $> 0$.
- Return standard `ProblemDetails` or formatted error responses on HTTP 400.

---

## Tier 3 Design Considerations: Database Persistence Tier & Concurrent Queue

### 1. Architecture & Non-Blocking Queue Flow
- **Decision**: Use `System.Threading.Channels.Channel<Trade>` (SingleReader, MultipleWriter) as an in-memory concurrent channel registered as a singleton service.
- **Write Pipeline**:
  1. `POST /trades` endpoint receives trade DTO, performs validation, assigns ID/Timestamp.
  2. Endpoint calls `_channel.Writer.TryWrite(trade)` (or `WriteAsync`) to enqueue the trade non-blockingly.
  3. API immediately responds to HTTP request (201 Created) without awaiting disk write/file locks.
  4. Background worker `TradePersistenceWorker` (`BackgroundService`) continuously reads from `_channel.Reader.ReadAllAsync()` and persists trades asynchronously to SQLite DB using EF Core.

### 2. SQLite Concurrency & File Lock Mitigation
- **Rationale**: Isolating all DB write operations to a single background worker thread reading sequentially from the concurrent channel completely eliminates SQLite database write locks, transaction collisions, and disk I/O bottlenecks on main API controller threads.

---

## Tier 2 Design Considerations: Frontend (Vue 3 + Pinia + Vite)

### 1. Architecture & State Management
- **Decision**: Centralize trade entry, blotter history, and positions summary inside a single Pinia store (`useTradeStore`).
- **Store Flow**:
  - `submitTrade(tradeData)` executes `POST /trades`, appends the returned trade to local `trades` array (newest first), and fetches or re-derives `positions`.
  - Guarantees instant UI reactive updates without full page reloads.

### 2. UI Layout & Scannability
- **Layout Grid**: Two-column layout on desktop:
  - **Left Pane**: Trade Entry Form (clean inputs, validation feedback, quick-action side toggle).
  - **Right Pane Top**: Reactive Positions Summary Panel.
  - **Right Pane Bottom**: Live Trade Blotter Table.
- **Scannability Visual Decisions**:
  - **Side Badges**: Vibrant Emerald/Green for `BUY`, Crimson/Red for `SELL`.
  - **Formatted Numbers**: Currency formatted as `$1,250.50`, quantities formatted with digit grouping (`1,000`). Short net quantities displayed clearly (e.g. `-100` or `100 Short`).
  - **Notional Value Column**: Explicitly calculated per row as `Quantity * Price`.
  - **Sorting**: Interactive header sorting on Symbol, Timestamp, Price, and Notional Value.

---

## Risks & Trade-offs

| Risk / Trade-off | Description | Mitigation Strategy |
| :--- | :--- | :--- |
| **REST vs Real-time Sync** | HTTP REST requires fetching positions post-trade submission rather than push-notifications. | Execute position refresh in Pinia store immediately upon `POST /trades` success for instant reactivity. |
| **SQLite File Concurrency** | SQLite file lock under heavy multi-threaded writes. | Single connection string with Write-Ahead Logging (WAL) enabled; acceptable for single-trader blotter application. |
| **Decimal Precision** | Floating point rounding issues with financial math. | Explicitly use `decimal` in C# and proper numeric rounding (`toFixed(2)`) in JavaScript/Vue display layer. |

---

## Decisions & Resolved Questions

1. **Short Positions**: **[RESOLVED]** Short positions ($\text{NetQty} < 0$) are fully permitted. Selling without prior buys creates a short position. Average cost reflects average short entry price for short positions, and weighted average math handles long-to-short and short-to-long position flips seamlessly.
2. **CORS & Port Assignment**: Default Vite dev server runs on `http://localhost:5173` and .NET 8 API on `http://localhost:5000/5200`. CORS policy will allow `http://localhost:5173` explicitly for seamless local dev.

