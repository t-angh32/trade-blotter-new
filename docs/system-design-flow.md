# System Architecture & Data Flow Diagram

This document outlines the full-stack system architecture and data flow for the Trade Blotter Application, highlighting the non-blocking API tier, in-memory trade cache, concurrent database queue persistence, dynamic position derivation logic, and reactive Vue 3 frontend components.

---

## 1. High-Level End-to-End Data Flow

```mermaid
flowchart TD
    subgraph Frontend ["Tier 1: Frontend UI (Vue 3 + Pinia + Vite)"]
        Form["TradeEntryForm.vue\n(Validation: Symbol, Side, Qty > 0, Price > 0)"]
        Store["Pinia Store\n(useTradeStore)"]
        BlotterGrid["TradeBlotter.vue\n(Scannable Grid, Buy/Sell Badges, Sortable)"]
        PosPanel["PositionsPanel.vue\n(Reactive Net Qty & Avg Cost Summary)"]
    end

    subgraph BackendAPI ["Tier 2: Backend API Tier (.NET 8 Web API)"]
        Controller["TradesController"]
        Cache["ITradeCacheService\n(In-Memory Trade Cache - Current Day Trades)"]
        PosService["PositionCalculatorService\n(Dynamic Net Qty & Avg Cost Math)"]
    end

    subgraph DatabaseTier ["Tier 3: Database Persistence Tier"]
        Channel["System.Threading.Channels.Channel\n(In-Memory Non-Blocking Queue)"]
        Worker["TradePersistenceWorker\n(BackgroundService)"]
        SQLiteDB[("SQLite Database\ntradeblotter.db")]
    end

    %% Startup Initialization
    Worker -.->|0. Startup Cache Seeding| Cache

    %% User Trade Submission Flow
    Form -->|1. Submit Trade Payload| Store
    Store -->|2. HTTP POST /trades| Controller
    Controller -->|3. Add to In-Memory Cache| Cache
    Controller -->|4. Non-Blocking Enqueue| Channel
    Controller -->|5. HTTP 201 Created| Store
    Store -->|6. Reactive Store Update| BlotterGrid

    %% Background Persistence Flow
    Channel -->|7. Dequeue Trade| Worker
    Worker -->|8. Async DB Write| SQLiteDB

    %% Fast Read Requests (Zero DB Hits)
    Store -->|9. HTTP GET /trades| Controller
    Controller -->|"10. Read Trades (Sub-ms)"| Cache
    Store -->|11. HTTP GET /positions| Controller
    Controller -->|12. Calculate Positions from Cache| PosService
    PosService -->|13. Read Cached Trades| Cache
    PosService -->|14. Return Positions Payload| Store
    Store -->|15. Reactive Render| PosPanel
```

---

## 2. Trade Submission & In-Memory Caching Sequence

```mermaid
sequenceDiagram
    autonumber
    actor Trader as Trader (User)
    participant UI as TradeEntryForm.vue
    participant Store as Pinia Store
    participant API as TradesController (POST /trades)
    participant Cache as ITradeCacheService (In-Memory)
    participant Channel as Channel<Trade> Queue
    participant Worker as TradePersistenceWorker
    participant DB as SQLite DB

    Trader->>UI: Enter Symbol, Side, Qty, Price & Click Submit
    UI->>UI: Perform Client-Side Validation
    UI->>Store: Dispatch submitTrade(payload)
    Store->>API: HTTP POST /trades (JSON Payload)
    API->>API: Validate Trade DTO & Assign UTC Timestamp + ID
    API->>Cache: AddTrade(trade) [Instant In-Memory Cache Insert]
    API->>Channel: Writer.TryWrite(trade) [Non-Blocking Queue Write]
    API-->>Store: HTTP 201 Created (Persisted Trade Object)
    Store->>Store: Append to trades state & update positions
    Store-->>UI: Clear form inputs & show success feedback

    par Background Persistence (Asynchronous)
        Channel->>Worker: Dequeue Trade Item (Reader.ReadAllAsync)
        Worker->>DB: TradeDbContext.Trades.AddAsync(trade)
        Worker->>DB: SaveChangesAsync() [Thread-Safe File Write]
    end
```

---

## 3. Fast Read Requests Sequence (GET /trades & GET /positions)

```mermaid
sequenceDiagram
    autonumber
    actor Trader as Trader (User)
    participant UI as Blotter / Positions View
    participant Store as Pinia Store
    participant API as TradesController
    participant Cache as ITradeCacheService (In-Memory)
    participant Calc as PositionCalculatorService

    note over API, Cache: Zero Database Access Required for Reads!

    rect rgb(235, 248, 255)
        note right of UI: Trade History Fetch (GET /trades)
        UI->>Store: Fetch Trades
        Store->>API: HTTP GET /trades
        API->>Cache: GetTodayTrades() [Sub-millisecond Read]
        Cache-->>API: List of Today's Trades (Newest First)
        API-->>Store: HTTP 200 OK (Trade Array JSON)
        Store-->>UI: Render Trade Blotter Table Grid
    end

    rect rgb(240, 255, 244)
        note right of UI: Derived Positions Fetch (GET /positions)
        UI->>Store: Fetch Positions
        Store->>API: HTTP GET /positions
        API->>Calc: CalculatePositions()
        Calc->>Cache: GetTodayTrades()
        Cache-->>Calc: List of Today's Trades
        Calc->>Calc: Compute Net Qty & Weighted Avg Cost per Symbol
        Calc-->>API: List of Derived Active Positions
        API-->>Store: HTTP 200 OK (Positions Array JSON)
        Store-->>UI: Render Positions Summary Panel
    end
```

---

## 4. Component Responsibility Matrix

| Tier | Component | Responsibilities |
| :--- | :--- | :--- |
| **Frontend UI** | `TradeEntryForm.vue` | Input capture, client validation (positive qty/price), submit triggering. |
| **Frontend UI** | `TradeBlotter.vue` | Displaying reverse-chronological trade history with visual Buy/Sell badges, notional value formatting (`Qty * Price`), and column header sorting. |
| **Frontend UI** | `PositionsPanel.vue` | Displaying reactive net positions, long/short indicators, and average execution costs per active symbol. |
| **Frontend UI** | `useTradeStore` (Pinia) | Centralized state management, API HTTP communication, and reactive store state updates. |
| **Backend API** | `TradesController` | REST endpoints (`POST /trades`, `GET /trades`, `GET /positions`), DTO validation, cache integration, queue submission. |
| **Backend API** | `ITradeCacheService` | In-memory thread-safe cache (`ConcurrentBag<Trade>` / `ConcurrentQueue<Trade>`) holding current day's trades for instant sub-millisecond reads. |
| **Backend API** | `PositionCalculatorService` | Dynamic derivation of net quantity and weighted average cost for long ($\text{NetQty} > 0$) and short ($\text{NetQty} < 0$) positions directly from cached trades. |
| **Database Tier** | `System.Threading.Channels.Channel<Trade>` | Thread-safe in-memory concurrent channel separating HTTP requests from disk write latency. |
| **Database Tier** | `TradePersistenceWorker` | `BackgroundService` executing single-threaded sequential writes to SQLite DB to prevent database file locking. |
| **Database Tier** | SQLite (`tradeblotter.db`) | Durable relational database store for trade records. |
