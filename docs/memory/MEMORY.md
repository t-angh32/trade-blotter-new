# Project Memory: Trade Blotter Application

## 1. Project Overview & Context
- **Project**: Trade Blotter Application Exercise
- **Goal**: Build a full-stack trade blotter application allowing traders to enter trades, view real-time trades in a blotter table, and observe dynamically calculated position summaries.
- **Target Audience**: Financial traders needing immediate blotter scannability and live net position feedback.

---

## 2. Technology Stack & Specifications

### Backend
- **Framework**: C# / .NET 8 (Web API)
- **In-Memory Cache**: `ITradeCacheService` maintaining current day's trades in memory for sub-millisecond `GET /trades` and `GET /positions` responses with zero DB disk I/O.
- **Database / Persistence**: SQLite (`tradeblotter.db`) with background persistence worker (`TradePersistenceWorker`) and non-blocking in-memory queue (`Channel<Trade>`).
- **ID Generator**: `ITradeIdGenerator` / `TradeIdGenerator` thread-safe singleton service initialized from `Max(Id)` in SQLite during startup.
- **UTC Timestamp Converter**: EF Core `ValueConverter` on `Trade.Timestamp` ensuring `DateTimeKind.Utc` is preserved upon reload from SQLite for standard ISO 8601 `Z` JSON serialization.
- **Endpoints**:
  - `POST /trades`: Validate trade, add immediately to in-memory trade cache, enqueue to concurrent channel, and return 201 Created without blocking on disk I/O.
  - `GET /trades`: Fetch current day's trades directly from in-memory trade cache.
  - `GET /positions`: Fetch derived positions calculated directly from cached trades.


### Frontend
- **Framework**: Vue 3 (Composition API)
- **State Management**: Pinia (`useTradeStore`)
- **Build Tool**: Vite
- **UI Components & Layout**:
  - **3-Column Trading Layout**: Compact fixed-size `New Trade Entry` (left), prominent `Live Trade Blotter` (center), expanded `Active Positions Summary` (right).
  - **100vh Viewport Layout**: Desktop SPA constrained to 100vh with vertical panel scrolling and sticky table headers.
  - **Trade Entry Form**: Symbol, Side (Buy/Sell), Quantity, Price with auto-focus after submit and global `Ctrl+Shift+E` focus hotkey.
  - **Blotter Table**: Live trade history (newest first by default) displaying Timestamp, Symbol, Side, Quantity, Price, and Notional Value with multi-column sorting via `Ctrl` + click and priority badges (`▲₁`, `▼₂`).
  - **Positions Panel**: Dynamically displays net quantity (supporting long & short positions) and average cost per symbol with multi-column sorting via `Ctrl` + click (`Symbol`, `Side`, `Net Qty`, `Avg Cost`).

---

## 3. Core Domain Models & Rules

### Trade Model
- `Symbol`: string (e.g., `AAPL`, `MSFT`)
- `Side`: string or enum (`Buy` / `Sell`)
- `Quantity`: decimal / integer (shares, > 0)
- `Price`: decimal (per share, > 0)
- `Timestamp`: DateTime (UTC)

### Position Derivation Rules
- **Derivation**: Positions **must not** be persisted separately in the database; they are calculated dynamically from trade history.
- **Net Quantity**: Sum of Buy quantities minus sum of Sell quantities.
- **Short Positions**: Fully permitted. If net quantity $< 0$, position represents an open short.
- **Average Cost**: Calculated based on trade execution history (handling mixed buys, sells, short entry prices, and covers).
- **Omission**: Symbols with a net position of zero must be omitted from position responses.

---

## 4. Key Decisions & Conventions
- **OpenSpec Integration**: Specifications, change tracking, and agent workflows are maintained under `docs/openspec/`.
- **Database Tier Decoupling**: API endpoints enqueue trades into an in-memory concurrent `Channel<Trade>` (SingleReader/MultipleWriter) and return immediately. A background worker persists trades asynchronously to SQLite, preventing disk write locking on HTTP threads.
- **Thread-Safe ID Generation**: Encapsulated in `ITradeIdGenerator` singleton service initialized at startup from `Max(Id)` in SQLite to prevent primary key collisions across application restarts.
- **UTC Timezone Preservation**: Configured EF Core `ValueConverter` for `Timestamp` (`DateTimeKind.Utc`) to ensure JSON payload timezone fidelity across restarts.
- **Multi-Column Grid Sorting**: Implemented `<kbd>Ctrl</kbd> + click` multi-column sorting with priority badges (`▲₁`, `▼₂`) across both Blotter and Position tables.
- **Short Positions**: Supported. Weighted average cost tracks entry price for short positions and adjusts seamlessly on long/short position flips.
- **Validation**: Strict input validation on frontend and backend for symbol presence and positive numerical values.
- **Testing Focus**: Unit tests primarily targeting position derivation, short position math, and average cost logic.
- **Session Rule**: At the start of every session, open `.\docs\memory\MEMORY.md` to get context on project progress and continuously append new developments.

---

## 5. Development Roadmap & Status
- [x] Initial project configuration & requirements import (`docs/requirements/trade-blotter-exercise.docx`).
- [x] OpenSpec agent configuration & directory structure setup.
- [x] Create project memory file structure at `.\docs\memory\MEMORY.md`.
- [x] Configure session memory persistence rules (`AGENTS.md`).
- [x] Configure `.\docs\openspec\.agents\skills` as workspace skills in `.\.agents\skills.json`.
- [x] Commit initial project documentation, agent guidelines, memory structure, and OpenSpec skills (`5193a8a`).
- [x] Formulate and commit OpenSpec change `trade-blotter-app` with proposal, sub-specs, technical design, and task breakdown (`0165161`, `4e8f699`, `b9ae778`).
- [x] Backend implementation (.NET 8 Web API + SQLite persistence + Channel worker + In-memory trade cache).
- [x] Frontend implementation (Vue 3 + Pinia + Vite).
- [x] Unit tests for position calculation and short position logic (8/8 xUnit tests passing).
- [x] Final verification, README setup instructions, and deployment readiness.

---

## 6. Progress History Log
- **2026-09-20**: Created memory file structure at `.\docs\memory\MEMORY.md`. Added workspace session instructions in `AGENTS.md` requiring the AI agent to inspect `.\docs\memory\MEMORY.md` at session start and maintain ongoing progress updates. Registered OpenSpec skills from `.\docs\openspec\.agents` persistently via `.\.agents\skills.json` and `.\.agents\skills\`.
- **2026-09-20**: Committed setup artifacts and workspace configurations to git repository (`5193a8a`). Memory file updated and synchronized. Ready to begin full-stack implementation.
- **2026-09-20**: Created OpenSpec change `trade-blotter-app` with sub-specs for backend API (`specs/backend-api/spec.md`) and frontend UI (`specs/frontend-ui/spec.md`), technical design (`design.md`), and tasks (`tasks.md`). Confirmed requirement that short positions ($\text{NetQty} < 0$) are permitted and updated calculation rules accordingly.
- **2026-09-20**: Added Database Persistence Tier capability to `proposal.md` and added sub-spec `specs/database-tier/spec.md`. The database tier introduces an in-memory concurrent queue (`Channel<Trade>`) and background worker (`TradePersistenceWorker`) to decouple API HTTP latency from SQLite disk writes and prevent locking. Updated `design.md` and `tasks.md`.
- **2026-09-20**: Committed OpenSpec change artifacts to branch `feature/00-design` (`0165161`). Evaluated 20k connection scalability considerations (SignalR, Redis, Kafka) and confirmed application target scope. Synchronized `MEMORY.md`.
- **2026-09-20**: Created system data flow diagram document at `.\docs\system-design-flow.md` with Mermaid diagrams illustrating end-to-end data flow, non-blocking queue submission, background persistence, dynamic position derivation steps, and component responsibilities.
- **2026-09-20**: Updated system architecture and OpenSpec planning files (`proposal.md`, `specs/backend-api/spec.md`, `design.md`, `tasks.md`, `system-design-flow.md`) to incorporate `ITradeCacheService`. Current day trades are cached in memory on `POST /trades` and served directly on `GET /trades` and `GET /positions` with zero database disk I/O hits on read requests.
- **2026-09-20**: Applied OpenSpec change `trade-blotter-app` (14/14 tasks complete). Built .NET 8 Web API (`src/TradeBlotter.Api`), SQLite `TradeDbContext`, `ITradeCacheService`, `Channel<Trade>` queue, `TradePersistenceWorker`, and `TradesController`. Created Vue 3 + Pinia + Vite frontend (`src/TradeBlotter.Web`). Wrote and verified 8 xUnit unit tests (`src/TradeBlotter.Tests`) with 100% pass rate. Verified full-stack integration and updated `README.md`.
- **2026-09-20**: Updated C# project files (`TradeBlotter.Api.csproj` and `TradeBlotter.Tests.csproj`) to explicitly target **`.NET 8.0`** (`net8.0`) per specification requirement. Verified build and xUnit test suite under `net8.0`.
- **2026-09-20**: Resolved SQLite primary key `UNIQUE constraint failed: Trades.Id` error. Refactored ID generation into a thread-safe singleton service (`ITradeIdGenerator` / `TradeIdGenerator`) injected into `TradesController.cs`, initialized from `Max(Id)` during startup in `Program.cs`, and configured `ValueGeneratedNever()` in `TradeDbContext.cs`. Cleaned up orphaned background process locks. Verified test suite (7/7 passing).
- **2026-09-20**: Confirmed and reinforced default sort order in `TradeBlotter.vue` to display most recent trades first (`timestamp` descending, with `b.id - a.id` secondary tie-breaker). Verified frontend Vite build.
- **2026-09-20**: Implemented multi-column sorting in `TradeBlotter.vue` by enabling `Ctrl` + click on column headers. Updated sort state to array of `SortRule` objects with priority indicators (e.g. `▲₁`, `▼₂`) in header labels. Verified frontend build and test suite.
- **2026-09-20**: Added automatic focus return to the `Symbol` input element (`symbolInputRef.value.focus()`) in `TradeEntryForm.vue` immediately after trade submission to streamline rapid trade entry. Verified Vite build.
- **2026-09-20**: Restructured frontend layout in `App.vue` and `style.css` to a 3-column widescreen grid (`310px 1fr 360px`). Positioned `Live Trade Blotter` in the center column between `New Trade Entry` (left) and `Active Positions Summary` (right) for optimal trading desk screen real-estate usage. Verified Vite build.
- **2026-09-20**: Expanded `Active Positions Summary` column width to `440px` in `style.css` and streamlined table header titles in `PositionsPanel.vue` (`Symbol`, `Side`, `Net Qty`, `Avg Cost`), completely eliminating horizontal scrollbars. Verified Vite build.
- **2026-09-20**: Constrained SPA height to 100vh on desktop viewports in `style.css` (`html, body { height: 100%; overflow: hidden; }`). Configured `.grid-layout`, `.card-panel`, and `.table-wrapper` with flexbox/grid container constraints so vertical scrollbars automatically appear within `Live Trade Blotter` and `Active Positions Summary` panels as trade/position lists grow or browser height is resized, with sticky table headers. Verified Vite build.
- **2026-09-20**: Configured `.col-entry` and `.col-entry .card-panel` with `height: auto` in `style.css` so `New Trade Entry` renders as a compact, fixed-size card matching its input form content, while the blotter and position panels remain full-height scrollable containers. Verified Vite build.
- **2026-09-20**: Implemented global keyboard shortcut `Ctrl+Shift+E` in `TradeEntryForm.vue` (`handleKeyDown`) to instantly focus and select the `Symbol` input field from anywhere in the application. Added shortcut hint badge to panel header. Verified Vite build.
- **2026-09-20**: Implemented multi-column sorting in `PositionsPanel.vue` by enabling `Ctrl` + click on column headers (`Symbol`, `Side`, `Net Qty`, `Avg Cost`). Added `PositionSortRule` state array, priority badges (`▲₁`, `▼₂`), and secondary symbol tie-breaker. Verified Vite build and xUnit test suite.
- **2026-09-20**: Fixed post-restart GMT timestamp display bug caused by EF Core SQLite reading timestamps as `DateTimeKind.Unspecified` (which caused `System.Text.Json` to omit the `Z` suffix and JS `new Date()` to parse UTC strings as local time). Added EF Core `ValueConverter` for `Timestamp` (`DateTime.SpecifyKind(v, DateTimeKind.Utc)`) in `TradeDbContext.cs` and defensive `Z` suffix normalization in `TradeBlotter.vue` (`formatDate`). Verified build and test suite.
- **2026-09-20**: Committed UI layout enhancements, multi-column grid sorting, auto-focus hotkeys, `ITradeIdGenerator` singleton, and UTC timestamp persistence fixes (`8602249`).
- **2026-09-20**: Reconciled commit hash references in `MEMORY.md` to align with rewritten git branch history and verified complete removal of restricted name references across all tracked files and commit logs.
- **2026-09-20**: Synced delta specs to main specs (`backend-api`, `database-tier`, `frontend-ui`) and archived completed OpenSpec change `trade-blotter-app` to `docs/openspec/openspec/changes/archive/2026-09-20-trade-blotter-app`.
- **2026-09-20**: Committed `README.md` documentation updates (`371f8f0`) highlighting structured logging, keyboard shortcuts (`Ctrl+Shift+E`), and multi-column grid sorting.











