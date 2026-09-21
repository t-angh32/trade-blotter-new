# Spec Delta

## ADDED Requirements

### Requirement: Real-Time SignalR Client Subscription and Live Blotter Updates
The system SHALL integrate `@microsoft/signalr` into the Vue 3 Pinia store (`useTradeStore`) to connect to `/hubs/trades` upon application initialization. The store MUST listen for incoming `TradeExecuted` events and automatically prepend new trades to the blotter state (`trades`) and refresh positions (`fetchPositions()`) across all open client browser windows without requiring manual page refreshes.

#### Scenario: Receive real-time trade event from external trade submission
- **WHEN** another browser instance or external REST client posts a trade
- **THEN** all connected browser clients receive `TradeExecuted`, prepend the trade to the live blotter table, and update position summaries reactively

#### Scenario: Automatic reconnection and resynchronization
- **WHEN** the SignalR connection drops due to network interruption or backend restart
- **THEN** `@microsoft/signalr` automatically attempts reconnection and re-fetches full trade history and positions (`loadAllData()`) upon re-establishing connection

### Requirement: SignalR Real-Time Connection Status Indicator
The system SHALL render a live connection status indicator in the UI header displaying the current SignalR connection state (`Connected`, `Reconnecting`, or `Disconnected`).

#### Scenario: Render connection state badge
- **WHEN** the SignalR HubConnection state changes
- **THEN** the header badge updates its text and visual status color (e.g. green for Connected, yellow for Reconnecting, red for Disconnected)
