<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useTradeStore } from '../stores/tradeStore';
import { TradeSide } from '../types';

const tradeStore = useTradeStore();

const form = reactive({
  symbol: '',
  side: TradeSide.Buy,
  quantity: null as number | null,
  price: null as number | null
});

const errors = reactive({
  symbol: '',
  quantity: '',
  price: ''
});

const successMsg = ref('');

const validate = (): boolean => {
  let isValid = true;
  errors.symbol = '';
  errors.quantity = '';
  errors.price = '';

  if (!form.symbol || !form.symbol.trim()) {
    errors.symbol = 'Symbol is required.';
    isValid = false;
  }

  if (form.quantity === null || form.quantity <= 0) {
    errors.quantity = 'Quantity must be greater than zero.';
    isValid = false;
  }

  if (form.price === null || form.price <= 0) {
    errors.price = 'Price must be greater than zero.';
    isValid = false;
  }

  return isValid;
};

const setSide = (side: TradeSide) => {
  form.side = side;
};

const handleSubmit = async () => {
  successMsg.value = '';
  if (!validate()) return;

  const success = await tradeStore.submitTrade({
    symbol: form.symbol.trim().toUpperCase(),
    side: form.side,
    quantity: Number(form.quantity),
    price: Number(form.price)
  });

  if (success) {
    successMsg.value = `Trade submitted: ${form.side} ${form.quantity} ${form.symbol.toUpperCase()} @ $${form.price}`;
    form.symbol = '';
    form.quantity = null;
    form.price = null;

    setTimeout(() => {
      successMsg.value = '';
    }, 4000);
  }
};
</script>

<template>
  <div class="card-panel">
    <div class="card-header">
      <h2 class="card-title">New Trade Entry</h2>
    </div>

    <form @submit.prevent="handleSubmit">
      <!-- Symbol Field -->
      <div class="form-group">
        <label class="form-label" for="symbol">Symbol</label>
        <input
          id="symbol"
          v-model="form.symbol"
          type="text"
          class="form-control"
          placeholder="e.g. AAPL, MSFT"
          autocomplete="off"
        />
        <div v-if="errors.symbol" class="error-msg">{{ errors.symbol }}</div>
      </div>

      <!-- Side Toggle -->
      <div class="form-group">
        <label class="form-label">Side</label>
        <div class="side-toggle">
          <button
            type="button"
            class="btn-side"
            :class="{ 'active-buy': form.side === TradeSide.Buy }"
            @click="setSide(TradeSide.Buy)"
          >
            BUY
          </button>
          <button
            type="button"
            class="btn-side"
            :class="{ 'active-sell': form.side === TradeSide.Sell }"
            @click="setSide(TradeSide.Sell)"
          >
            SELL
          </button>
        </div>
      </div>

      <!-- Quantity Field -->
      <div class="form-group">
        <label class="form-label" for="quantity">Quantity (Shares)</label>
        <input
          id="quantity"
          v-model.number="form.quantity"
          type="number"
          step="any"
          min="0.0001"
          class="form-control"
          placeholder="e.g. 100"
        />
        <div v-if="errors.quantity" class="error-msg">{{ errors.quantity }}</div>
      </div>

      <!-- Price Field -->
      <div class="form-group">
        <label class="form-label" for="price">Price ($)</label>
        <input
          id="price"
          v-model.number="form.price"
          type="number"
          step="0.01"
          min="0.01"
          class="form-control"
          placeholder="e.g. 150.50"
        />
        <div v-if="errors.price" class="error-msg">{{ errors.price }}</div>
      </div>

      <!-- Error / Success Notification -->
      <div v-if="tradeStore.error" class="error-msg" style="margin-bottom: 0.75rem;">
        {{ tradeStore.error }}
      </div>
      <div v-if="successMsg" class="side-badge buy" style="width: 100%; text-align: center; margin-bottom: 0.75rem; padding: 0.5rem;">
        ✓ {{ successMsg }}
      </div>

      <!-- Submit Button -->
      <button type="submit" class="btn-submit" :disabled="tradeStore.submitting">
        {{ tradeStore.submitting ? 'Submitting Trade...' : 'Submit Trade' }}
      </button>
    </form>
  </div>
</template>
