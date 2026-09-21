# Spec Delta: Frontend UI Tier

## Purpose

Delivers a responsive Vue 3 (Composition API) single-page user interface using Pinia for centralized state management, enabling immediate trade entry, scannable live trade blotter, and reactive position tracking.

## ADDED Requirements

### Requirement: Trade Entry Form Component
The system SHALL provide a Vue 3 trade entry form component with inputs for `Symbol`, `Side` (Buy/Sell toggle/select), `Quantity`, and `Price`. The component MUST validate that `Symbol` is non-empty, and `Quantity` and `Price` are greater than zero before submitting. Upon successful submission, the form MUST reset and trigger an immediate update of blotter and positions state without a full page reload.

#### Scenario: Successful trade submission from UI
- **WHEN** the trader fills in valid Symbol ("AAPL"), Side ("Buy"), Quantity (100), and Price (150.00) and clicks Submit
- **THEN** a POST request is sent to `/trades`, the form fields clear, and the blotter and positions components update reactively

#### Scenario: Trade entry client validation error
- **WHEN** the trader attempts to submit with an empty Symbol or zero/negative Quantity or Price
- **THEN** submission is prevented, and inline validation error messages are displayed next to the invalid fields

### Requirement: Scannable Trade Blotter Grid Component
The system SHALL provide a trade blotter table component displaying all trades in reverse chronological order (newest first). Columns MUST include `Timestamp`, `Symbol`, `Side`, `Quantity`, `Price`, and `Notional Value` (`Quantity * Price`). The grid MUST visually distinguish Buy vs Sell sides (e.g. via color coding/badges) and support sorting by at least one column (such as Timestamp or Symbol).

#### Scenario: Blotter view rendering and sorting
- **WHEN** trades are loaded into the Pinia store
- **THEN** the blotter table renders each trade with calculated Notional Value, distinct Buy/Sell styling, and provides column header click sorting

### Requirement: Reactive Positions Summary Panel Component
The system SHALL provide a position summary panel component displaying current `Net Quantity` and `Average Cost` per symbol. The panel MUST reactively update when new trades are entered or loaded. Symbols with a net quantity of zero MUST NOT be displayed.

#### Scenario: Reactive position display update
- **WHEN** a new trade is submitted and confirmed by the API
- **THEN** the positions summary panel automatically updates net quantity and average cost for the affected symbol without manual browser refresh
