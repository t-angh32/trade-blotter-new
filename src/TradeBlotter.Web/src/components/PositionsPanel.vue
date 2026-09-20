<script setup lang="ts">
import { ref, computed } from 'vue';
import { useTradeStore } from '../stores/tradeStore';
import type { Position } from '../types';

const tradeStore = useTradeStore();

type PositionSortField = 'symbol' | 'side' | 'netQuantity' | 'averageCost';
type SortOrder = 'asc' | 'desc';

interface PositionSortRule {
  field: PositionSortField;
  order: SortOrder;
}

const sortRules = ref<PositionSortRule[]>([
  { field: 'symbol', order: 'asc' }
]);

const handleHeaderClick = (field: PositionSortField, event: MouseEvent) => {
  const isMultiSort = event.ctrlKey || event.metaKey;
  const existingIdx = sortRules.value.findIndex(r => r.field === field);

  if (isMultiSort) {
    if (existingIdx >= 0) {
      sortRules.value[existingIdx].order = sortRules.value[existingIdx].order === 'asc' ? 'desc' : 'asc';
    } else {
      sortRules.value.push({ field, order: 'asc' });
    }
  } else {
    if (existingIdx === 0 && sortRules.value.length === 1) {
      sortRules.value[0].order = sortRules.value[0].order === 'asc' ? 'desc' : 'asc';
    } else {
      sortRules.value = [{ field, order: 'asc' }];
    }
  }
};

const getSortInfo = (field: PositionSortField) => {
  const idx = sortRules.value.findIndex(r => r.field === field);
  if (idx === -1) return null;
  return {
    order: sortRules.value[idx].order,
    priority: sortRules.value.length > 1 ? idx + 1 : null
  };
};

const sortedPositions = computed(() => {
  const list = [...tradeStore.positions];
  return list.sort((a, b) => {
    for (const rule of sortRules.value) {
      let valA: any;
      let valB: any;

      if (rule.field === 'side') {
        valA = a.netQuantity > 0 ? 'LONG' : (a.netQuantity < 0 ? 'SHORT' : 'FLAT');
        valB = b.netQuantity > 0 ? 'LONG' : (b.netQuantity < 0 ? 'SHORT' : 'FLAT');
      } else {
        valA = a[rule.field as keyof Position];
        valB = b[rule.field as keyof Position];
      }

      if (typeof valA === 'string') {
        valA = valA.toLowerCase();
        valB = (valB as string).toLowerCase();
      }

      if (valA < valB) return rule.order === 'asc' ? -1 : 1;
      if (valA > valB) return rule.order === 'asc' ? 1 : -1;
    }

    return a.symbol.localeCompare(b.symbol);
  });
});

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
      <div>
        <h2 class="card-title">Active Positions Summary</h2>
        <div style="font-size: 0.75rem; color: var(--text-muted); margin-top: 0.125rem;">
          Hold <kbd style="background: rgba(255,255,255,0.1); padding: 0.1rem 0.3rem; border-radius: 3px;">Ctrl</kbd> + click column headers to multi-sort
        </div>
      </div>
      <div style="font-size: 0.8125rem; color: var(--text-muted);">
        {{ tradeStore.positions.length }} Open Position(s)
      </div>
    </div>

    <div class="table-wrapper">
      <table class="data-table">
        <thead>
          <tr>
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
            <th class="sortable" @click="handleHeaderClick('netQuantity', $event)">
              Net Qty
              <span v-if="getSortInfo('netQuantity')">
                {{ getSortInfo('netQuantity')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('netQuantity')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('netQuantity')?.priority }}</sub>
              </span>
            </th>
            <th class="sortable" @click="handleHeaderClick('averageCost', $event)">
              Avg Cost
              <span v-if="getSortInfo('averageCost')">
                {{ getSortInfo('averageCost')?.order === 'asc' ? '▲' : '▼' }}<sub v-if="getSortInfo('averageCost')?.priority" style="font-size: 0.65rem; margin-left: 2px;">{{ getSortInfo('averageCost')?.priority }}</sub>
              </span>
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="pos in sortedPositions" :key="pos.symbol">
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
          <tr v-if="sortedPositions.length === 0">
            <td colspan="4" class="empty-state">
              No active positions. (Symbols with net zero position are omitted).
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
