<script setup lang="ts">
import { ref, computed } from 'vue';
import { useTradeStore } from '../stores/tradeStore';
import type { Trade } from '../types';

const tradeStore = useTradeStore();

type SortField = 'timestamp' | 'symbol' | 'side' | 'quantity' | 'price' | 'notional';
type SortOrder = 'asc' | 'desc';

interface SortRule {
  field: SortField;
  order: SortOrder;
}

const sortRules = ref<SortRule[]>([
  { field: 'timestamp', order: 'desc' }
]);

const handleHeaderClick = (field: SortField, event: MouseEvent) => {
  const isMultiSort = event.ctrlKey || event.metaKey;
  const existingIdx = sortRules.value.findIndex(r => r.field === field);

  if (isMultiSort) {
    if (existingIdx >= 0) {
      sortRules.value[existingIdx].order = sortRules.value[existingIdx].order === 'asc' ? 'desc' : 'asc';
    } else {
      sortRules.value.push({ field, order: 'desc' });
    }
  } else {
    if (existingIdx === 0 && sortRules.value.length === 1) {
      sortRules.value[0].order = sortRules.value[0].order === 'asc' ? 'desc' : 'asc';
    } else {
      sortRules.value = [{ field, order: 'desc' }];
    }
  }
};

const getSortInfo = (field: SortField) => {
  const idx = sortRules.value.findIndex(r => r.field === field);
  if (idx === -1) return null;
  return {
    order: sortRules.value[idx].order,
    priority: sortRules.value.length > 1 ? idx + 1 : null
  };
};

const formatNotional = (trade: Trade): number => {
  return trade.quantity * trade.price;
};

const sortedTrades = computed(() => {
  const list = [...tradeStore.trades];
  return list.sort((a, b) => {
    for (const rule of sortRules.value) {
      let valA: any;
      let valB: any;

      if (rule.field === 'notional') {
        valA = formatNotional(a);
        valB = formatNotional(b);
      } else if (rule.field === 'timestamp') {
        valA = new Date(a.timestamp).getTime();
        valB = new Date(b.timestamp).getTime();
      } else {
        valA = a[rule.field as keyof Trade];
        valB = b[rule.field as keyof Trade];
      }

      if (typeof valA === 'string') {
        valA = valA.toLowerCase();
        valB = (valB as string).toLowerCase();
      }

      if (valA < valB) return rule.order === 'asc' ? -1 : 1;
      if (valA > valB) return rule.order === 'asc' ? 1 : -1;
    }

    // Secondary tie-breaker: newest trade ID first
    return b.id - a.id;
  });
});

const formatCurrency = (val: number): string => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(val);
};

const formatNumber = (val: number): string => {
  return new Intl.NumberFormat('en-US', { maximumFractionDigits: 4 }).format(val);
};

const formatSide = (side: any): string => {
  if (side === 0 || side === '0' || String(side).toLowerCase() === 'buy') return 'Buy';
  if (side === 1 || side === '1' || String(side).toLowerCase() === 'sell') return 'Sell';
  return String(side);
};

const formatDate = (dateStr: string): string => {
  try {
    const normalizedStr = (dateStr.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(dateStr))
      ? dateStr
      : dateStr + 'Z';
    const d = new Date(normalizedStr);
    return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' }) +
           ' (' + d.toLocaleDateString() + ')';
  } catch {
    return dateStr;
  }
};
</script>

<template>
  <div class="card-panel">
    <div class="card-header">
      <div>
        <h2 class="card-title">Live Trade Blotter</h2>
        <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.125rem;">
          Hold <kbd style="background: rgba(255,255,255,0.1); padding: 0.1rem 0.3rem; border-radius: 3px;">Ctrl</kbd> + click column headers to multi-sort
        </div>
      </div>
      <div style="font-size: 0.8125rem; color: var(--text-muted);">
        {{ tradeStore.trades.length }} Execution(s) Today
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
            <th class="sortable" @click="handleHeaderClick('timestamp', $event)">
              Timestamp
              <span v-if="getSortInfo('timestamp')">
                {{ getSortInfo('timestamp')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('timestamp')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('timestamp')?.priority }}</sub>
              </span>
            </th>
            <th class="sortable" @click="handleHeaderClick('symbol', $event)">
              Symbol
              <span v-if="getSortInfo('symbol')">
                {{ getSortInfo('symbol')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('symbol')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('symbol')?.priority }}</sub>
              </span>
            </th>
            <th class="sortable" @click="handleHeaderClick('side', $event)">
              Side
              <span v-if="getSortInfo('side')">
                {{ getSortInfo('side')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('side')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('side')?.priority }}</sub>
              </span>
            </th>
            <th class="sortable" @click="handleHeaderClick('quantity', $event)">
              Quantity
              <span v-if="getSortInfo('quantity')">
                {{ getSortInfo('quantity')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('quantity')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('quantity')?.priority }}</sub>
              </span>
            </th>
            <th class="sortable" @click="handleHeaderClick('price', $event)">
              Price
              <span v-if="getSortInfo('price')">
                {{ getSortInfo('price')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('price')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('price')?.priority }}</sub>
              </span>
            </th>
            <th class="sortable" @click="handleHeaderClick('notional', $event)">
              Notional Value
              <span v-if="getSortInfo('notional')">
                {{ getSortInfo('notional')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('notional')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('notional')?.priority }}</sub>
              </span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="trade in sortedTrades" :key="trade.id">
            <td style="font-family: monospace; font-size: 0.8125rem;">{{ formatDate(trade.timestamp) }}</td>
            <td style="font-weight: 700; color: #ffffff;">{{ trade.symbol }}</td>
            <td>
              <span class="side-badge" :class="formatSide(trade.side).toLowerCase()">
                {{ formatSide(trade.side).toUpperCase() }}
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
