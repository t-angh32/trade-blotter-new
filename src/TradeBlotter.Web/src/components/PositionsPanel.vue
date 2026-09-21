<script setup lang="ts">
import { useTradeStore } from '../stores/tradeStore';

const tradeStore = useTradeStore();

const formatCurrency = (val: number): string => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(val);
};

const formatNumber = (val: number): string => {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 4 }).format(val);
};
</script>

<template>
  <div class="card-panel">
    <div class="card-header">
      <h2 class="card-title">Active Positions Summary</h2>
      <div style="font-size: 0.8125rem; color: var(--text-muted);">
        {{ tradeStore.positions.length }} Open Position(s)
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th>Symbol</th>
            <th>Side / Exposure</th>
            <th>Net Quantity</th>
            <th>Weighted Avg Cost</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="pos in tradeStore.positions" :key="pos.symbol">
            <td style="font-weight: 700; color: #ffffff;">{{ pos.symbol }}</td>
            <td>
              <span v-if="pos.netQuantity > 0" class="side-badge buy">LONG</span>
              <span v-else-if="pos.netQuantity < 0" class="side-badge sell">SHORT</span>
            </td>
            <td :class="{ 'short-indicator': pos.netQuantity < 0 }">
              {{ formatNumber(pos.netQuantity) }}
            </td>
            <td style="font-weight: 600;">{{ formatCurrency(pos.averageCost) }}</td>
          </tr>
          <tr v-if="tradeStore.positions.length === 0">
            <td colspan="4" class="empty-state">
              No active positions. (Symbols with net zero position are omitted).
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
