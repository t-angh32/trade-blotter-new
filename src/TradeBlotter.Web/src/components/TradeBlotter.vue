<script setup lang="ts">
import { ref, computed } from 'vue';
import { useTradeStore } from '../stores/tradeStore';
import type { Trade } from '../types';

const tradeStore = useTradeStore();

type SortField = 'timestamp' | 'symbol' | 'side' | 'quantity' | 'price' | 'notional';
type SortOrder = 'asc' | 'desc';

const sortField = ref<SortField>('timestamp');
const sortOrder = ref<SortOrder>('desc');

const toggleSort = (field: SortField) => {
  if (sortField.value === field) {
    sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortField.value = field;
    sortOrder.value = 'desc';
  }
};

const formatNotional = (trade: Trade): number => {
  return trade.quantity * trade.price;
};

const sortedTrades = computed(() => {
  const list = [...tradeStore.trades];
  return list.sort((a, b) => {
    let valA: any;
    let valB: any;

    if (sortField.value === 'notional') {
      valA = formatNotional(a);
      valB = formatNotional(b);
    } else if (sortField.value === 'timestamp') {
      valA = new Date(a.timestamp).getTime();
      valB = new Date(b.timestamp).getTime();
    } else {
      valA = a[sortField.value as keyof Trade];
      valB = b[sortField.value as keyof Trade];
    }

    if (typeof valA === 'string') {
      valA = valA.toLowerCase();
      valB = (valB as string).toLowerCase();
    }

    if (valA < valB) return sortOrder.value === 'asc' ? -1 : 1;
    if (valA > valB) return sortOrder.value === 'asc' ? 1 : -1;
    return 0;
  });
});

const formatCurrency = (val: number): string => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(val);
};

const formatNumber = (val: number): string => {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 4 }).format(val);
};

const formatDate = (dateStr: string): string => {
  try {
    const d = new Date(dateStr);
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' }) +
           ' (' + d.toLocaleDateString() + ')';
  } catch {
    return dateStr;
  }
};
</script>

<template>
  <div class="card-panel" style="margin-top: 1.5rem;">
    <div class="card-header">
      <h2 class="card-title">Live Trade Blotter</h2>
      <div style="font-size: 0.8125rem; color: var(--text-muted);">
        {{ tradeStore.trades.length }} Execution(s) Today
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th class="sortable" @click="toggleSort('timestamp')">
              Timestamp {{ sortField === 'timestamp' ? (sortOrder === 'asc' ? '▲' : '▼') : '' }}
            </th>
            <th class="sortable" @click="toggleSort('symbol')">
              Symbol {{ sortField === 'symbol' ? (sortOrder === 'asc' ? '▲' : '▼') : '' }}
            </th>
            <th class="sortable" @click="toggleSort('side')">
              Side {{ sortField === 'side' ? (sortOrder === 'asc' ? '▲' : '▼') : '' }}
            </th>
            <th class="sortable" @click="toggleSort('quantity')">
              Quantity {{ sortField === 'quantity' ? (sortOrder === 'asc' ? '▲' : '▼') : '' }}
            </th>
            <th class="sortable" @click="toggleSort('price')">
              Price {{ sortField === 'price' ? (sortOrder === 'asc' ? '▲' : '▼') : '' }}
            </th>
            <th class="sortable" @click="toggleSort('notional')">
              Notional Value {{ sortField === 'notional' ? (sortOrder === 'asc' ? '▲' : '▼') : '' }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="trade in sortedTrades" :key="trade.id">
            <td style="font-family: monospace; font-size: 0.8125rem;">{{ formatDate(trade.timestamp) }}</td>
            <td style="font-weight: 700; color: #ffffff;">{{ trade.symbol }}</td>
            <td>
              <span class="side-badge" :class="String(trade.side).toLowerCase()">
                {{ String(trade.side).toUpperCase() }}
              </span>
            </td>
            <td>{{ formatNumber(trade.quantity) }}</td>
            <td>{{ formatCurrency(trade.price) }}</td>
            <td style="font-weight: 600;">{{ formatCurrency(formatNotional(trade)) }}</td>
          </tr>
          <tr v-if="sortedTrades.length === 0">
            <td colspan="6" class="empty-state">
              No trades executed today. Submit a trade above to populate the blotter.
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
