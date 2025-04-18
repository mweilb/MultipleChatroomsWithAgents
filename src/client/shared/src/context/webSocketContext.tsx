import React,  {
  createContext,
  ReactNode,
  useCallback,
  useEffect,
  useRef,
  useState,
} from 'react';

import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketReplyChatRoomMessage } from '../models/WebSocketReplyChatRoomMessages';
import { WebSocketGetRoomsMessage, WebSocketRoom } from '../models/WebSocketGetRoomsMessage';
import { WebSocketAudioMessage } from '../models/WebSocketVoiceMessage';
import { WebSocketNewRoomMessage } from '../models/WebSocketNewRoomMessage';
import { WebSocketModeration } from '../models/WebSocketModerationRequest';

import { useMessageStore } from '../hooks/useMessageStore';
import { useRoomStore } from '../hooks/useRoomStore';
import { useVoice } from '../hooks/useVoice';
import { useModeration } from '../hooks/useModeration';
import { useErrorStore } from '../hooks/useErrorStore';
import { WebSocketLibrariansMessage } from '../models/WebSocketGetLibrarians';
import { useLibrariansStore } from '../hooks/useLibrariansStore';
import { WebSocketLibrarianConverse } from '../models/WebSocketLibrarianConverse';
import { WebSocketLibrarianList } from '../models/WebSocketLibrarianList';
import { useEditorMode } from '../hooks/useEditorMode';

export interface IWebSocketContext {
  getMessages(action: string): WebSocketReplyChatRoomMessage[];
  getRequestState(action: string): string;
  sendMessage: (message: WebSocketBaseMessage, orchestrator: string, room: string) => void;
  addOrUpdateMessage: (message: WebSocketReplyChatRoomMessage) => void;
  connectionStatus: 'Connected' | 'Disconnected' | 'Reconnecting';
  rooms: WebSocketRoom[];
  setAudioMessageListener: (listener: (msg: WebSocketAudioMessage) => void) => void;
  requestNewRoomListener: (listener: (msg: WebSocketNewRoomMessage) => void) => void;
  requestRoomChange: (userId:string, group: string, to: string) => void;
  toggleVoice: (isOn: boolean) => void;
  setModeratorMessageListener: (listener: (msg: WebSocketModeration) => void) => void;
  moderationHistory: { [room: string]: WebSocketModeration[] };
  errorHistory: { [room: string]: WebSocketBaseMessage[] };
  resetChat: (room: string) => void;
  // New librarians store properties:
  library: WebSocketLibrariansMessage | null;
  sendConverse: (roomName: string, AgentName: string, text: string) => void;
  getLibrarianConverseMessages: (roomName: string, agentName: string) => WebSocketLibrarianConverse[];
  sendList: (roomName: string, AgentName: string, text: string) => void;
  getLibrarianListMessages: (roomName: string, agentName: string) => WebSocketLibrarianList[];
  askForDocs: (roomName: string, AgentName: string, top: number, skip: number) => void;
  getLibrarianDocs: (roomName: string, agentName: string) => WebSocketLibrarianList;
}

// Using a default value of null for clarity.
export const WebSocketContext = createContext<IWebSocketContext | null>(null);

interface WebSocketProviderProps {
  url: string;
  appType: string;
  retryInterval?: number;
  maxRetries?: number;
  children: ReactNode;
}

export const WebSocketProvider = ({
  url,
  appType,
  retryInterval = 5000,
  maxRetries = 10,
  children,
}: WebSocketProviderProps) : React.ReactElement   => {
  const [websocket, setWebsocket] = useState<WebSocket | null>(null);
  const [connectionStatus, setConnectionStatus] = useState<'Connected' | 'Disconnected' | 'Reconnecting'>('Disconnected');
  const reconnectAttempts = useRef(0);
  const reconnectTimer = useRef<number | null>(null);

  const { errorHistory, handleErrorMessage, resetErrors } = useErrorStore();
  const { moderationHistory, setModeratorMessageListener, handleModerationMessage } = useModeration();

  const sender = useCallback(
    (message: WebSocketBaseMessage) => {
      if (websocket && websocket.readyState === WebSocket.OPEN) {
        websocket.send(JSON.stringify(message));
      } else {
        console.error('WebSocket is not open. Message not sent:', message);
      }
    },
    [websocket]
  );

  const librariansStore = useLibrariansStore(sender);
  const { 
    library: currentLibrary, 
    requestLibrary: triggerLibraryRequest, 
    handleLibrariansMessage: processLibrariansMessage,
    sendConverse, 
    handleLibrarianConverseMessage,
    getLibrarianConverseMessages,
    sendList,
    handleLibrarianListMessage,
    getLibrarianListMessages,
    askForDocs,
    handleLibrarianDocMessage,
    getLibrarianDocs
  } = librariansStore;

  const { getMessages, addOrUpdateMessage, resetMessagesForRoom, sendMessage, getRequestState } = useMessageStore(sender);

  const {
    rooms,
    updateRooms,
    changeRoom,
    resetRoom,
    setNewRoomListener,
    handleNewRoomMessage,
    triggerRoomsRequest,
  } = useRoomStore(sender);

  const { appType: currentAppType, triggerEditorMode } = useEditorMode(appType);
  const { toggleVoice, handleAudioMessage, setAudioMessageListener } = useVoice(sender);

  // Heartbeat/ping-pong logic
  const heartbeatInterval = 15000; // 15 seconds
  const pongTimeout = 5000; // 5 seconds
  const heartbeatTimer = useRef<number | null>(null);
  const pongTimer = useRef<number | null>(null);

  const clearHeartbeat = () => {
    if (heartbeatTimer.current) clearInterval(heartbeatTimer.current);
    if (pongTimer.current) clearTimeout(pongTimer.current);
  };

  const initializeWebSocket = () => {
    const socketConnection = new WebSocket(url);

    const sendPing = () => {
      if (socketConnection.readyState === WebSocket.OPEN) {
        socketConnection.send(JSON.stringify({ Action: "ping" }));
        pongTimer.current = window.setTimeout(() => {
          console.warn("Pong not received, closing socket.");
          socketConnection.close();
        }, pongTimeout);
      }
    };

    socketConnection.onopen = () => {
      console.log('WebSocket connected');
      setConnectionStatus('Connected');
      reconnectAttempts.current = 0;

      // Start heartbeat
      heartbeatTimer.current = window.setInterval(sendPing, heartbeatInterval);

      if (currentAppType === 'editor') {
        triggerEditorMode(socketConnection);
      }
      triggerRoomsRequest(socketConnection);
      triggerLibraryRequest(socketConnection);
    };
 
    // Handler map for action/subaction routing
    const messageHandlers: {
      [action: string]: {
        [subAction: string]: (msg: any) => void;
      } | ((msg: any) => void);
    } = {
      editor: () => {},
      ModeResponse: () => {},
      rooms: (msg: WebSocketGetRoomsMessage) => {
        updateRooms(msg.Rooms ?? []);
      },
      librarians: (msg: any) => {
        processLibrariansMessage(msg);
      },
      librarian: {
        'converse-message': (msg: WebSocketLibrarianConverse) => {
          handleLibrarianConverseMessage(msg);
        },
        list: (msg: WebSocketLibrarianList) => {
          handleLibrarianListMessage(msg);
        },
        doc: (msg: WebSocketLibrarianList) => {
          handleLibrarianDocMessage(msg);
        },
      },
      moderator: (msg: WebSocketModeration) => {
        handleModerationMessage(msg);
      },
      error: (msg: any) => {
        handleErrorMessage(msg);
      },
      audio: (msg: WebSocketAudioMessage) => {
        handleAudioMessage(msg);
      },
    };

    socketConnection.onmessage = (event) => {
      let incomingMessage: any | null = null;
      try {
        incomingMessage = JSON.parse(event.data);
      } catch (err) {
        console.error('Invalid JSON:', event.data);
        return;
      }
      if (incomingMessage.Action === "pong") {
        // Pong received, clear pong timeout
        if (pongTimer.current) clearTimeout(pongTimer.current);
        return;
      }
      if (incomingMessage.Action === 'unknown') {
        console.error('Unknown message sent:', incomingMessage);
        return;  
      }
      if (
        !incomingMessage ||
        !incomingMessage.UserId ||
        !incomingMessage.TransactionId ||
        !incomingMessage.Action ||
        !incomingMessage.SubAction
      ) {
        console.error('Malformed message:', incomingMessage);
        return;
      }

      // Handler dispatch logic
      const actionHandler = messageHandlers[incomingMessage.Action];
      if (typeof actionHandler === 'function') {
        actionHandler(incomingMessage);
      } else if (
        actionHandler &&
        typeof actionHandler === 'object' &&
        incomingMessage.SubAction &&
        actionHandler[incomingMessage.SubAction]
      ) {
        actionHandler[incomingMessage.SubAction](incomingMessage);
      } else if (
        incomingMessage.SubAction === 'change-room' ||
        incomingMessage.SubAction === 'change-room-yield'
      ) {
        const roomMessageChange = incomingMessage as WebSocketNewRoomMessage;
        handleNewRoomMessage(roomMessageChange);
        addOrUpdateMessage(incomingMessage);
      } else {
        addOrUpdateMessage(incomingMessage);
      }
    };

    socketConnection.onclose = () => {
      console.log('WebSocket disconnected');
      setConnectionStatus('Reconnecting');
      clearHeartbeat();
      handleReconnect();
    };

    socketConnection.onerror = (err) => {
      if (reconnectAttempts.current >= maxRetries) {
        console.error('WebSocket error:', err);
      }
      clearHeartbeat();
      socketConnection.close();
    };

    setWebsocket(socketConnection);
  };

  // Exponential backoff for reconnection
  const handleReconnect = () => {
    if (reconnectAttempts.current < maxRetries) {
      reconnectAttempts.current += 1;
      const backoff = Math.min(
        retryInterval * Math.pow(2, reconnectAttempts.current - 1),
        60000
      ); // Cap at 60s
      console.log(`Reconnecting attempt ${reconnectAttempts.current}/${maxRetries}, waiting ${backoff}ms`);
      reconnectTimer.current = window.setTimeout(initializeWebSocket, backoff);
    } else {
      console.error('Max reconnection attempts reached.');
      setConnectionStatus('Disconnected');
    }
  };

  useEffect(() => {
    initializeWebSocket();
    return () => {
      if (websocket) websocket.close();
      if (reconnectTimer.current) clearTimeout(reconnectTimer.current);
      clearHeartbeat();
    };
  }, [url]);

  const resetChat = (room: string) => {
    resetMessagesForRoom(room);
    resetErrors(room);
    resetRoom(room);
  };
 
  return (
    <WebSocketContext.Provider
      value={{
        getMessages,
        getRequestState,
        sendMessage,
        addOrUpdateMessage,
        connectionStatus,
        rooms,
        setAudioMessageListener,
        requestNewRoomListener: setNewRoomListener,
        requestRoomChange: changeRoom,
        toggleVoice,
        setModeratorMessageListener,
        moderationHistory,
        errorHistory,
        resetChat,
        sendConverse,
        library: currentLibrary,
        getLibrarianConverseMessages,
        sendList,
        getLibrarianListMessages,
        askForDocs,
        getLibrarianDocs,
      }}
    >
      {children}
    </WebSocketContext.Provider>
  );
};
