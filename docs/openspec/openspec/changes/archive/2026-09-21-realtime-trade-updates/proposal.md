# Proposal

## Why

Currently, when a trade is posted from one browser window or entered via external REST API calls (e.g. cURL, Postman, command-line scripts), other open browser instances do not receive real-time updates. Traders relying on open blotter screens are left with stale trade history and inaccurate position summaries until a manual page refresh occurs. Real-time push updates via SignalR are required to ensure all connected trade blotter clients immediately receive live trade executions and position recalculations.

## What Changes

- **SignalR Real-Time Hub**: Introduce an ASP.NET Core SignalR hub (`TradeHub`) mapped to `/hubs/trades` on the backend Web API.
- **Backend Real-Time Broadcast**: Broadcast `TradeExecuted` SignalR events to all connected clients whenever a new trade is submitted via `POST /trades`.
- **Frontend Real-Time Subscription**: Integrate `@microsoft/signalr` package into the Vue 3 Pinia store (`useTradeStore`) to establish a persistent WebSocket/SignalR connection with automatic reconnect capabilities.
- **Reactive Multi-Client Updates**: Update the Pinia store upon receiving `TradeExecuted` events so live blotter tables and position summary panels refresh immediately across all open browser instances.
- **Connection Status Indicator**: Display a visual SignalR connectivity indicator (`Connected`, `Reconnecting`, `Disconnected`) in the UI header.

## Capabilities

### Modified Capabilities
- `backend-api`: Add SignalR real-time Hub mapping (`/hubs/trades`), `IHubContext<TradeHub>` broadcasting on `POST /trades`, and CORS credentials configuration.
- `frontend-ui`: Add `@microsoft/signalr` HubConnection management, live trade event listener, automatic reconnection, and live connection badge.

## Impact

- **Backend**: `Program.cs`, `TradesController.cs`, addition of `TradeHub.cs` under `Hubs/`, CORS policy updated for SignalR WebSockets.
- **Frontend**: `package.json` (`@microsoft/signalr`), `src/stores/tradeStore.ts`, `src/App.vue`.
