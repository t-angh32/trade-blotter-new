export enum TradeSide {
  Buy = 'Buy',
  Sell = 'Sell'
}

export interface Trade {
  id: number;
  symbol: string;
  side: TradeSide | string;
  quantity: number;
  price: number;
  timestamp: string;
}

export interface CreateTradePayload {
  symbol: string;
  side: TradeSide;
  quantity: number;
  price: number;
}

export interface Position {
  symbol: string;
  netQuantity: number;
  averageCost: number;
}
