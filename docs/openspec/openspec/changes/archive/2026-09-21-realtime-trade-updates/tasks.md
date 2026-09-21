# Tasks

## 1. Backend SignalR Integration

- [x] 1.1 Create `TradeHub` in `src/TradeBlotter.Api/Hubs/TradeHub.cs` and verify project compilation
- [x] 1.2 Register SignalR service (`builder.Services.AddSignalR()`), map hub endpoint (`app.MapHub<TradeHub>("/hubs/trades")`), and update CORS policy to support credentials in `Program.cs`
- [x] 1.3 Inject `IHubContext<TradeHub>` into `TradesController.cs` and broadcast `TradeExecuted` event on `POST /trades`
- [x] 1.4 Verify backend builds without errors and xUnit test suite passes cleanly (`dotnet test`)

## 2. Frontend SignalR Integration

- [x] 2.1 Add `@microsoft/signalr` package dependency to `src/TradeBlotter.Web/package.json` and verify installation
- [x] 2.2 Update Pinia store (`src/TradeBlotter.Web/src/stores/tradeStore.ts`) to manage SignalR `HubConnection`, listen for `TradeExecuted` events, deduplicate incoming trades, auto-reconnect, and trigger position updates
- [x] 2.3 Add visual connection status badge (`Connected`, `Reconnecting`, `Disconnected`) to `src/TradeBlotter.Web/src/App.vue` header
- [x] 2.4 Verify frontend Vite build succeeds (`npm run build`)

## 3. End-to-End Verification

- [x] 3.1 Perform multi-client verification: post a trade via external REST API/cURL or secondary browser session and confirm all open blotter browser instances update live trades and positions in real-time
