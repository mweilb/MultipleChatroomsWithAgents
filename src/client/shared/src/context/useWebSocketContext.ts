import { useContext } from 'react';
import { IWebSocketContext, WebSocketContext } from './webSocketContext';

export const useWebSocketContext = (): IWebSocketContext => {
  const context = useContext(WebSocketContext);
  if (!context) {
    throw new Error('useWebSocketContext must be used within a WebSocketProvider');
  }
  return context;
};
