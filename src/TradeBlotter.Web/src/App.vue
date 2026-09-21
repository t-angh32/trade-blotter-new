<script setup lang="ts">
import { onMounted } from 'vue';
import { useTradeStore } from './stores/tradeStore';
import TradeEntryForm from './components/TradeEntryForm.vue';
import PositionsPanel from './components/PositionsPanel.vue';
import TradeBlotter from './components/TradeBlotter.vue';

const tradeStore = useTradeStore();

onMounted(async () => {
  await tradeStore.loadAllData();
});
</script>

<template>
  <div class="app-container">
    <header class="app-header">
      <div class="header-title">
        <h1>Trade Blotter</h1>
        <span class="badge-trading-desk">Live Desk</span>
        <span
          class="badge-connection"
          :class="{
            'status-connected': tradeStore.connectionStatus === 'Connected',
            'status-reconnecting': tradeStore.connectionStatus === 'Reconnecting',
            'status-disconnected': tradeStore.connectionStatus === 'Disconnected'
          }"
          :title="`SignalR Status: ${tradeStore.connectionStatus}`"
        >
          <span class="status-dot"></span>
          {{ tradeStore.connectionStatus }}
        </span>
      </div>
      <div style="display: flex; gap: 0.5rem; align-items: center;">
        <button
          type="button"
          class="btn-side"
          style="padding: 0.4rem 0.8rem; font-size: 0.8125rem;"
          @click="tradeStore.loadAllData()"
        >
          🔄 Refresh
        </button>
      </div>
    </header>

    <main class="grid-layout">
      <!-- Column 1: New Trade Entry -->
      <aside class="col-entry">
        <TradeEntryForm />
      </aside>

      <!-- Column 2: Live Trade Blotter -->
      <section class="col-blotter">
        <TradeBlotter />
      </section>

      <!-- Column 3: Active Positions Summary -->
      <aside class="col-positions">
        <PositionsPanel />
      </aside>
    </main>
  </div>
</template>
