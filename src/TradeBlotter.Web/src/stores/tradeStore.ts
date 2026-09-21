import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import type { Trade, CreateTradePayload, Position } from '../types';

const API_BASE_URL = 'http://localhost:5000';

export type SignalRConnectionStatus = 'Connected' | 'Reconnecting' | 'Disconnected';

export const useTradeStore = defineStore('tradeStore', () => {
  const trades = ref<Trade[]>([]);
  const positions = ref<Position[]>([]);
  const loading = ref<boolean>(false);
  const submitting = ref<boolean>(false);
  const error = ref<string | null>(null);
  const connectionStatus = ref<SignalRConnectionStatus>('Disconnected');

  let hubConnection: HubConnection | null = null;

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
      
      // Immediately update local trades list with deduplication check
      if (!trades.value.some(t => t.id === response.data.id)) {
        trades.value = [response.data, ...trades.value];
      }
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

  const initSignalR = () => {
    if (hubConnection) return;

    hubConnection = new HubConnectionBuilder()
      .withUrl(`${API_BASE_URL}/hubs/trades`)
      .withAutomaticReconnect()
      .build();

    hubConnection.onreconnecting(() => {
      connectionStatus.value = 'Reconnecting';
    });

    hubConnection.onreconnected(() => {
      connectionStatus.value = 'Connected';
      // Refresh full data upon reconnection to ensure state accuracy
      loadAllData();
    });

    hubConnection.onclose(() => {
      connectionStatus.value = 'Disconnected';
    });

    hubConnection.on('TradeExecuted', (trade: any) => {
      // Normalize trade side enum if sent as numeric index
      if (trade.side === 0 || trade.side === '0') trade.side = 'Buy';
      if (trade.side === 1 || trade.side === '1') trade.side = 'Sell';

      // Deduplicate trade if already present in state
      if (!trades.value.some(t => t.id === trade.id)) {
        trades.value = [trade, ...trades.value];
      }
      fetchPositions();
    });

    hubConnection.start()
      .then(() => {
        connectionStatus.value = 'Connected';
      })
      .catch(err => {
        console.error('SignalR Hub Connection Error:', err);
        connectionStatus.value = 'Disconnected';
      });
  };

  const loadAllData = async () => {
    await Promise.all([fetchTrades(), fetchPositions()]);
    initSignalR();
  };

  return {
    trades,
    positions,
    loading,
    submitting,
    error,
    connectionStatus,
    fetchTrades,
    fetchPositions,
    submitTrade,
    loadAllData,
    initSignalR
  };
});
