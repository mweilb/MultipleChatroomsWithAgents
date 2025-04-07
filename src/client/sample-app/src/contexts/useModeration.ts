import { useState, useRef, useCallback } from 'react';
import { WebSocketModeration } from '../models/WebSocketModerationRequest';

export function useModeration() {
  // Store moderation messages per room.
  const [moderationHistory, setModerationHistory] = useState<{ [room: string]: WebSocketModeration[] }>({});
  
  // Ref for an optional external moderator message listener.
  const moderatorMessageListenerRef = useRef<((msg: WebSocketModeration) => void) | null>(null);

  // Setter function to register an external moderator listener.
  const setModeratorMessageListener = useCallback((listener: (msg: WebSocketModeration) => void) => {
    moderatorMessageListenerRef.current = listener;
  }, []);

  // Handler function that processes an incoming moderation message.
  const handleModerationMessage = useCallback((msg: WebSocketModeration) => {
    const room = msg.SubAction || 'all';
    setModerationHistory((prev) => {
      const existing = prev[room] || [];
      return { ...prev, [room]: [...existing, msg] };
    });
    if (moderatorMessageListenerRef.current) {
      moderatorMessageListenerRef.current(msg);
    } else {
      console.warn('No moderator message listener registered.');
    }
  }, []);

  return { moderationHistory, setModeratorMessageListener, handleModerationMessage };
}
