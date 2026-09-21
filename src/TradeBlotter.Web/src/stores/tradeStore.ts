import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';
import type { Trade, CreateTradePayload, Position } from '../types';

const API_BASE_URL = 'http://localhost:5000';

export const useTradeStore = defineStore('tradeStore', () => {
  const trades = ref<Trade[]>([]);
  const positions = ref<Position[]>([]);
  const loading = ref<boolean>(false);
  const submitting = ref<boolean>(false);
  const error = ref<string | null>(null);

  const fetchTrades = async () => {
    try {
      loading.value = true;
      error.value = null;
      const response = await axios.get<Trade[]>(`${API_BASE_URL}/trades`);
      trades.value = response.data;
    } catch (err: any) {
      console.error('Failed to fetch trades:', err);
      error.value = err?.response?.data?.title || 'Failed to fetch trade history.';
    } finally {
      loading.value = false;
    }
  };

  const fetchPositions = async () => {
    try {
      error.value = null;
      const response = await axios.get<Position[]>(`${API_BASE_URL}/positions`);
      positions.value = response.data;
    } catch (err: any) {
      console.error('Failed to fetch positions:', err);
      error.value = err?.response?.data?.title || 'Failed to fetch positions summary.';
    }
  };

  const submitTrade = async (payload: CreateTradePayload): Promise<boolean> => {
    try {
      submitting.value = true;
      error.value = null;
      const response = await axios.post<Trade>(`${API_BASE_URL}/trades`, payload);
      
      // Immediately update local trades list and refresh positions
      trades.value = [response.data, ...trades.value];
      await fetchPositions();
      return true;
    } catch (err: any) {
      console.error('Failed to submit trade:', err);
      error.value = err?.response?.data?.detail || err?.response?.data?.title || 'Failed to submit trade execution.';
      return false;
    } finally {
      submitting.value = false;
    }
  };

  const loadAllData = async () => {
    await Promise.all([fetchTrades(), fetchPositions()]);
  };

  return {
    trades,
    positions,
    loading,
    submitting,
    error,
    fetchTrades,
    fetchPositions,
    submitTrade,
    loadAllData
  };
});
