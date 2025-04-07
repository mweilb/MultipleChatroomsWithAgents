import { useRef, useCallback } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketAudioMessage } from '../models/WebSocketVoiceMessage';

export function useVoice(sender: (message: WebSocketBaseMessage) => void) {
  
  // Internal ref for an optional external audio message listener.
  const audioMessageListenerRef = useRef<((msg: WebSocketAudioMessage) => void) | null>(null);

  // Expose a setter so external code (if needed) can register a listener.
  const setAudioMessageListener = useCallback((listener: (msg: WebSocketAudioMessage) => void) => {
    audioMessageListenerRef.current = listener;
  }, []);

  // Function that processes incoming audio messages.
  // This function will be called by the WebSocket context on receiving an audio message.
  const handleAudioMessage = useCallback((msg: WebSocketAudioMessage) => {
    
    if (audioMessageListenerRef.current) {
      audioMessageListenerRef.current(msg);
    }
  }, []);

  // Toggle voice on/off by sending a message using the provided sender function.
  const toggleVoice = useCallback(
    (on: boolean) => {
      const subAction = on ? 'on' : 'off';
      const message: WebSocketBaseMessage = {
        UserId: '',
        TransactionId: 'voice-toggle-' + Date.now(),
        Action: 'voice',
        SubAction: subAction,
        Content: '',
        RoomName: '',
        SubRoomName: '',
        Mode: 'app',
      };
      sender(message);
    },
    [sender]
  );

  return {
    toggleVoice,
    handleAudioMessage, // Call this when an audio message arrives.
    setAudioMessageListener,
  };
}
