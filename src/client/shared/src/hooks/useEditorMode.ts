import { useState } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';

export function useEditorMode(mode: string) {
  // Initialize editorMode state with a default value of "app"
  const [appType,setAppType] = useState<string>(mode && mode.trim() !== '' ? mode : 'app');

  // Optionally accept a mode parameter; if it's empty, default to "app"
  const triggerEditorMode = (socket: WebSocket): void => {
 
    // Create a message to request the editor mode change.
    const requestEditorMode: WebSocketBaseMessage = {
      UserId: 'app',
      TransactionId: 'mode-' + Date.now(),
      Action: 'mode',
      SubAction: appType,
      Content: '',  
      RoomName: '',
      SubRoomName: '',
      Mode: 'app',
    };

    // Send the request message.
    socket.send(JSON.stringify(requestEditorMode));
  };

  return {
    triggerEditorMode,
    appType,
    setAppType
  };
}
