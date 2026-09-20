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
- **Endpoints**:
  - `POST /trades`: Validate trade, add immediately to in-memory trade cache, enqueue to concurrent channel, and return 201 Created without blocking on disk I/O.
  - `GET /trades`: Fetch current day's trades directly from in-memory trade cache.
  - `GET /positions`: Fetch derived positions calculated directly from cached trades.


### Frontend
- **Framework**: Vue 3 (Composition API)
- **State Management**: Pinia (`useTradeStore`)
- **Build Tool**: Vite
- **UI Components**:
  - **Trade Entry Form**: Symbol, Side (Buy/Sell), Quantity, Price with validation (non-empty symbol, positive quantity/price).
  - **Blotter Table**: Live trade history (newest first) displaying Timestamp, Symbol, Side, Quantity, Price, and Notional Value with visual Buy/Sell badges and column sorting.
  - **Positions Panel**: Dynamically displays net quantity (supporting long & short positions) and average cost per symbol; updates reactively upon trade entry.

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
- [x] Commit initial project documentation, agent guidelines, memory structure, and OpenSpec skills (`df4026f`).
- [x] Formulate and commit OpenSpec change `trade-blotter-app` with proposal, sub-specs, technical design, and task breakdown (`47c5318`).
- [ ] Backend implementation (.NET 8 Web API + SQLite persistence + Channel worker).
- [ ] Frontend implementation (Vue 3 + Pinia + Vite).
- [ ] Unit tests for position calculation and short position logic.
- [ ] Final verification, README setup instructions, and deployment readiness.

---

## 6. Progress History Log
- **2026-09-20**: Created memory file structure at `.\docs\memory\MEMORY.md`. Added workspace session instructions in `AGENTS.md` requiring the AI agent to inspect `.\docs\memory\MEMORY.md` at session start and maintain ongoing progress updates. Registered OpenSpec skills from `.\docs\openspec\.agents` persistently via `.\.agents\skills.json` and `.\.agents\skills\`.
- **2026-09-20**: Committed setup artifacts and workspace configurations to git repository (`df4026f`). Memory file updated and synchronized. Ready to begin full-stack implementation.
- **2026-09-20**: Created OpenSpec change `trade-blotter-app` with sub-specs for backend API (`specs/backend-api/spec.md`) and frontend UI (`specs/frontend-ui/spec.md`), technical design (`design.md`), and tasks (`tasks.md`). Confirmed requirement that short positions ($\text{NetQty} < 0$) are permitted and updated calculation rules accordingly.
- **2026-09-20**: Added Database Persistence Tier capability to `proposal.md` and added sub-spec `specs/database-tier/spec.md`. The database tier introduces an in-memory concurrent queue (`Channel<Trade>`) and background worker (`TradePersistenceWorker`) to decouple API HTTP latency from SQLite disk writes and prevent locking. Updated `design.md` and `tasks.md`.
- **2026-09-20**: Committed OpenSpec change artifacts to branch `feature/00-design` (`47c5318`). Evaluated 20k connection scalability considerations (SignalR, Redis, Kafka) and confirmed application target scope. Synchronized `MEMORY.md`.
- **2026-09-20**: Created system data flow diagram document at `.\docs\system-design-flow.md` with Mermaid diagrams illustrating end-to-end data flow, non-blocking queue submission, background persistence, dynamic position derivation steps, and component responsibilities.
- **2026-09-20**: Updated system architecture and OpenSpec planning files (`proposal.md`, `specs/backend-api/spec.md`, `design.md`, `tasks.md`, `system-design-flow.md`) to incorporate `ITradeCacheService`. Current day trades are cached in memory on `POST /trades` and served directly on `GET /trades` and `GET /positions` with zero database disk I/O hits on read requests.





