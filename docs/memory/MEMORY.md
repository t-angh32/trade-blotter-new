# Project Memory: Trade Blotter Application

## 1. Project Overview & Context
- **Project**: Trade Blotter Application Exercise
- **Goal**: Build a full-stack trade blotter application allowing traders to enter trades, view real-time trades in a blotter table, and observe dynamically calculated position summaries.
- **Target Audience**: Financial traders needing immediate blotter scannability and live net position feedback.

---

## 2. Technology Stack & Specifications

### Backend
- **Framework**: C# / .NET 8 (Web API)
- **Database / Persistence**: SQLite or SQL Server LocalDB
- **Endpoints**:
  - `POST /trades`: Submit a new trade record.
  - `GET /trades`: Fetch all trades, sorted newest first.
  - `GET /positions`: Fetch derived positions (Net Quantity, Average Cost per Symbol).

### Frontend
- **Framework**: Vue 3 (Composition API)
- **State Management**: Pinia
- **Build Tool**: Vite
- **UI Components**:
  - **Trade Entry Form**: Symbol, Side (Buy/Sell), Quantity, Price with validation (non-empty symbol, positive quantity/price).
  - **Blotter Table**: Live trade history (newest first) displaying Timestamp, Symbol, Side, Quantity, Price, and Notional Value.
  - **Positions Panel**: Dynamically displays net quantity and average cost per symbol; updates reactively upon trade entry.

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
- **Average Cost**: Calculated based on trade execution history (handling mixed buys and sells).
- **Omission**: Symbols with a net position of zero must be omitted from position responses.

---

## 4. Key Decisions & Conventions
- **OpenSpec Integration**: Specifications, change tracking, and agent workflows are maintained under `docs/openspec/`.
- **Validation**: Strict input validation on frontend and backend for symbol presence and positive numerical values.
- **Testing Focus**: Unit tests primarily targeting position derivation and average cost logic.
- **Session Rule**: At the start of every session, open `.\docs\memory\MEMORY.md` to get context on project progress and continuously append new developments.

---

## 5. Development Roadmap & Status
- [x] Initial project configuration & requirements import (`docs/requirements/trade-blotter-exercise.docx`).
- [x] OpenSpec agent configuration & directory structure setup.
- [x] Create project memory file structure at `.\docs\memory\MEMORY.md`.
- [x] Configure session memory persistence rules (`AGENTS.md`).
- [x] Configure `.\docs\openspec\.agents\skills` as workspace skills in `.\.agents\skills.json`.
- [x] Commit initial project documentation, agent guidelines, memory structure, and OpenSpec skills (`df4026f`).
- [ ] Backend implementation (.NET 8 Web API + SQLite/LocalDB persistence).
- [ ] Frontend implementation (Vue 3 + Pinia + Vite).
- [ ] Unit tests for position calculation logic.
- [ ] Final verification, README setup instructions, and deployment readiness.

---

## 6. Progress History Log
- **2026-09-20**: Created memory file structure at `.\docs\memory\MEMORY.md`. Added workspace session instructions in `AGENTS.md` requiring the AI agent to inspect `.\docs\memory\MEMORY.md` at session start and maintain ongoing progress updates. Registered OpenSpec skills from `.\docs\openspec\.agents` persistently via `.\.agents\skills.json` and `.\.agents\skills\`.
- **2026-09-20**: Committed setup artifacts and workspace configurations to git repository (`df4026f`). Memory file updated and synchronized. Ready to begin full-stack implementation.
- **2026-09-20**: Created OpenSpec change `trade-blotter-app` with sub-specs for backend API (`specs/backend-api/spec.md`) and frontend UI (`specs/frontend-ui/spec.md`), technical design (`design.md`), and tasks (`tasks.md`). Confirmed requirement that short positions ($\text{NetQty} < 0$) are permitted and updated calculation rules accordingly.

