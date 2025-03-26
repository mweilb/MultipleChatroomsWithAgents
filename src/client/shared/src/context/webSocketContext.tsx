import React,  {
  createContext,
  ReactNode,
  useCallback,
  useContext,
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

interface IWebSocketContext {
  getMessages(action: string): WebSocketReplyChatRoomMessage[];
  sendMessage: (message: WebSocketBaseMessage) => void;
  connectionStatus: 'Connected' | 'Disconnected' | 'Reconnecting';
  rooms: WebSocketRoom[];
  setAudioMessageListener: (listener: (msg: WebSocketAudioMessage) => void) => void;
  requestNewRoomListener: (listener: (msg: WebSocketNewRoomMessage) => void) => void;
  requestRoomChange: (group: string, to: string) => void;
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
const WebSocketContext = createContext<IWebSocketContext | null>(null);

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

  const { getMessages, addOrUpdateMessage, resetMessagesForRoom, sendMessage } = useMessageStore(sender);

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

  const initializeWebSocket = () => {
    const socketConnection = new WebSocket(url);
    socketConnection.onopen = () => {
      console.log('WebSocket connected');
      setConnectionStatus('Connected');
      reconnectAttempts.current = 0;

      if (currentAppType === 'editor') {
        triggerEditorMode(socketConnection);
      }
      triggerRoomsRequest(socketConnection);
      triggerLibraryRequest(socketConnection);
    };

    socketConnection.onmessage = (event) => {
      let incomingMessage: any | null = null;
      try {
        incomingMessage = JSON.parse(event.data);
      } catch (err) {
        console.error('Invalid JSON:', event.data);
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
      if (incomingMessage.Action === 'rooms') {
        const roomsResponse = incomingMessage as WebSocketGetRoomsMessage;
        updateRooms(roomsResponse.Rooms ?? []);
      } else if (incomingMessage.Action === 'librarians') {
        processLibrariansMessage(incomingMessage);
      } else if (
        incomingMessage.Action === 'librarian' &&
        incomingMessage.SubAction === 'converse-message'
      ) {
        handleLibrarianConverseMessage(incomingMessage as WebSocketLibrarianConverse);
      } else if (
        incomingMessage.Action === 'librarian' &&
        incomingMessage.SubAction === 'list'
      ) {
        handleLibrarianListMessage(incomingMessage as WebSocketLibrarianList);
      } else if (
        incomingMessage.Action === 'librarian' &&
        incomingMessage.SubAction === 'doc'
      ) {
        handleLibrarianDocMessage(incomingMessage as WebSocketLibrarianList);
      } else if (incomingMessage.Action === 'moderator') {
        const moderatorMessage = incomingMessage as WebSocketModeration;
        handleModerationMessage(moderatorMessage);
      } else if (incomingMessage.Action === 'error') {
        handleErrorMessage(incomingMessage);
      } else if (incomingMessage.Action === 'audio') {
        const audioMessage = incomingMessage as WebSocketAudioMessage;
        handleAudioMessage(audioMessage);
      } else if (incomingMessage.SubAction === 'change-room') {
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
      handleReconnect();
    };

    socketConnection.onerror = (err) => {
      console.error('WebSocket error:', err);
      socketConnection.close();
    };

    setWebsocket(socketConnection);
  };

  const handleReconnect = () => {
    if (reconnectAttempts.current < maxRetries) {
      reconnectAttempts.current += 1;
      console.log(`Reconnecting attempt ${reconnectAttempts.current}/${maxRetries}`);
      reconnectTimer.current = window.setTimeout(initializeWebSocket, retryInterval);
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
        sendMessage,
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

export const useWebSocketContext = (): IWebSocketContext => {
  const context = useContext(WebSocketContext);
  if (!context) {
    throw new Error('useWebSocketContext must be used within a WebSocketProvider');
  }
  return context;
};
