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
      <!-- Left Column: Trade Entry Form -->
      <aside>
        <TradeEntryForm />
      </aside>

      <!-- Right Column: Positions Panel & Live Blotter -->
      <section>
        <PositionsPanel />
        <TradeBlotter />
      </section>
    </main>
  </div>
</template>
