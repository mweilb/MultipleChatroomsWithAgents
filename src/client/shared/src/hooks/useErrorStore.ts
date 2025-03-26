import { useState, useCallback } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';

export function useErrorStore() {
  const [errorHistory, setErrorHistory] = useState<{ [room: string]: WebSocketBaseMessage[] }>({});

  const handleErrorMessage = useCallback((msg: WebSocketBaseMessage) => {
    const room = msg.SubAction || 'all';
    setErrorHistory(prev => {
      const existing = prev[room] || [];
      return { ...prev, [room]: [...existing, msg] };
    });
    console.error('Error message:', msg);
  }, []);

  const resetErrors = useCallback((room: string) => {
    setErrorHistory(prev => ({ ...prev, [room]: [] }));
  }, []);

  return { errorHistory, handleErrorMessage, resetErrors };
}
