import React, {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from 'react';

// Import model types for messages
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketReplyChatRoomMessage } from '../models/WebSocketReplyChatRoomMessages';
import { WebSocketGetRoomsMessage, WebSocketRoom } from '../models/WebSocketGetRoomsMessage';
import { WebSocketAudioMessage } from '../models/WebSocketVoiceMessage';
import { WebSocketNewRoomMessage } from '../models/WebSocketNewRoomMessage';
import { WebSocketModeration } from '../models/WebSocketModerationRequest';

// Import our custom hooks that encapsulate specific features
import { useMessageStore } from '../hooks/useMessageStore';
import { useRoomStore } from '../hooks/useRoomStore';
import { useVoice } from '../hooks/useVoice';
import { useModeration } from '../hooks/useModeration';
import { useErrorStore } from '../hooks/useErrorStore';
import { WebSocketLibrariansMessage } from '../models/WebSocketGetLibrarians';
import { useLibrariansStore } from '../hooks/useLibrariansStore';
import { WebSocketLibrarianConverse } from '../models/WebSocketLibrarianConverse';
import { WebSocketLibrarianList } from '../models/WebSocketLibrarianList';
 

// Define the shape of our WebSocket context; these are the functions and state values
// that will be available to any component using our WebSocket context.
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
  askForDocs: (roomName: string, AgentName: string, top: number, skip:number) => void;
  getLibrarianDocs: (roomName: string, agentName: string) => WebSocketLibrarianList;

 
}

// Create a React context to share our WebSocket functionalities
const WebSocketContext = createContext<IWebSocketContext | undefined>(undefined);

// Define the props for our provider component.
// The provider wraps parts of our app that need access to WebSocket functionalities.
interface WebSocketProviderProps {
  url: string; // WebSocket URL
  retryInterval?: number; // How long to wait before trying to reconnect (in milliseconds)
  maxRetries?: number; // Maximum number of reconnection attempts
  children: ReactNode; // Child components that will use this context
}

// The WebSocketProvider component that initializes and manages our WebSocket connection
export const WebSocketProvider: React.FC<WebSocketProviderProps> = ({
  url,
  retryInterval = 5000,
  maxRetries = 10,
  children,
}) => {
  // State to store the WebSocket instance
  const [websocket, setWebsocket] = useState<WebSocket | null>(null);
  // State to track the connection status (Connected, Disconnected, or Reconnecting)
  const [connectionStatus, setConnectionStatus] = useState<'Connected' | 'Disconnected' | 'Reconnecting'>('Disconnected');
  // useRef to track how many reconnection attempts have been made
  const reconnectAttempts = useRef(0);
  // useRef to store the timer ID for reconnection attempts
  const reconnectTimer = useRef<number | null>(null);

  // Use our error store hook to manage error messages from the WebSocket.
  const { errorHistory, handleErrorMessage, resetErrors } = useErrorStore();

  // Use our moderation hook to manage moderator messages.
  const { moderationHistory, setModeratorMessageListener, handleModerationMessage } = useModeration();

  // Create a sender function that sends messages over the WebSocket.
  // This function is passed to our other hooks so they can send messages as needed.
  const sender = useCallback(
    (message: WebSocketBaseMessage) => {
      if (websocket && websocket.readyState === WebSocket.OPEN) {
        websocket.send(JSON.stringify(message)); // Convert the message to a JSON string and send it.
      } else {
        console.error('WebSocket is not open. Message not sent:', message);
      }
    },
    [websocket]
  );

  // Use our librarians store hook to manage the library (librarians) data.
  // We pass in our sender so that this hook can send a request message.
  const librariansStore = useLibrariansStore(sender);
  // Destructure properties from our librarians store:
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

  // Use our message store hook to manage chat messages.
  const { getMessages, addOrUpdateMessage, resetMessagesForRoom, sendMessage } = useMessageStore(sender);

  // Use our room store hook to manage room-related operations.
  const {
    rooms,
    updateRooms,
    changeRoom,       // Function to change the current room.
    resetRoom,        // Function to reset room data.
    setNewRoomListener, // Function to register a listener for new room messages.
    handleNewRoomMessage, // Function to handle incoming "change-room" messages.
    triggerRoomsRequest, // Function to request the list of available rooms.
  } = useRoomStore(sender);

  // Use our voice hook to manage voice functionalities.
  const { toggleVoice, handleAudioMessage, setAudioMessageListener } = useVoice(sender);

  // Function to initialize the WebSocket connection.
  const initializeWebSocket = () => {
    // Create a new WebSocket connection using the provided URL.
    const socketConnection = new WebSocket(url);

    // When the connection opens:
    socketConnection.onopen = () => {
      console.log('WebSocket connected');
      setConnectionStatus('Connected');
      reconnectAttempts.current = 0; // Reset reconnection attempts


      triggerRoomsRequest(socketConnection); // Request room data immediately on connection.
      // Optionally, you could request library data immediately on connection:
      triggerLibraryRequest(socketConnection);
    };

    // When a message is received from the server:
    socketConnection.onmessage = (event) => {
      let incomingMessage: any | null = null;
      try {
        // Parse the incoming data as JSON.
        incomingMessage = JSON.parse(event.data);
      } catch (err) {
        console.error('Invalid JSON:', event.data);
        return;
      }
      // Check if the message has the required properties.
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
      // Process the message based on its "Action" property.
      if (incomingMessage.Action === 'rooms') {
        // Handle room data messages.
        const roomsResponse = incomingMessage as WebSocketGetRoomsMessage;
        updateRooms(roomsResponse.Rooms ?? []);
      } else if (incomingMessage.Action === 'librarians') {
        // Handle librarian (library) messages.
        processLibrariansMessage(incomingMessage);
      } 
      else if (
        incomingMessage.Action === 'librarian' &&
        incomingMessage.SubAction === 'converse-message'
      ) {
        // NEW: Handle librarian conversation messages
        // Cast to proper type if needed and pass to the handler
        handleLibrarianConverseMessage(incomingMessage as WebSocketLibrarianConverse);
      } 
      else if (
        incomingMessage.Action === 'librarian' &&
        incomingMessage.SubAction === 'list'
      ) {
        // NEW: Handle librarian conversation messages
        // Cast to proper type if needed and pass to the handler
        handleLibrarianListMessage(incomingMessage as WebSocketLibrarianList);
      } 
      else if (
        incomingMessage.Action === 'librarian' &&
        incomingMessage.SubAction === 'doc'
      ) {
        // NEW: Handle librarian conversation messages
        // Cast to proper type if needed and pass to the handler
        handleLibrarianDocMessage(incomingMessage as WebSocketLibrarianList);
      } 
      else if (incomingMessage.Action === 'moderator') {
        // Handle moderator messages.
        const moderatorMessage = incomingMessage as WebSocketModeration;
        handleModerationMessage(moderatorMessage);
      } else if (incomingMessage.Action === 'error') {
        // Handle error messages.
        handleErrorMessage(incomingMessage);
      } else if (incomingMessage.Action === 'audio') {
        // Handle audio messages.
        const audioMessage = incomingMessage as WebSocketAudioMessage;
        handleAudioMessage(audioMessage);
      } else if (incomingMessage.SubAction === 'change-room') {
        // Handle room change messages.
        const roomMessageChange = incomingMessage as WebSocketNewRoomMessage;
        handleNewRoomMessage(roomMessageChange);
        // Also update our general message store.
        addOrUpdateMessage(incomingMessage);
      } else {
        // For any other message types, simply update the message store.
        addOrUpdateMessage(incomingMessage);
      }
    };

    // When the connection closes:
    socketConnection.onclose = () => {
      console.log('WebSocket disconnected');
      setConnectionStatus('Reconnecting');
      handleReconnect();
    };

    // When there is an error with the connection:
    socketConnection.onerror = (err) => {
      console.error('WebSocket error:', err);
      socketConnection.close();
    };

    // Save the WebSocket connection in state so it can be used elsewhere.
    setWebsocket(socketConnection);
  };

  // Function to handle reconnection attempts if the connection is lost.
  const handleReconnect = () => {
    if (reconnectAttempts.current < maxRetries) {
      reconnectAttempts.current += 1;
      console.log(`Reconnecting attempt ${reconnectAttempts.current}/${maxRetries}`);
      // Try to reconnect after a delay defined by retryInterval.
      reconnectTimer.current = window.setTimeout(initializeWebSocket, retryInterval);
    } else {
      console.error('Max reconnection attempts reached.');
      setConnectionStatus('Disconnected');
    }
  };

  // Set up the WebSocket connection when this component mounts.
  useEffect(() => {
    initializeWebSocket();
    // Clean up the connection when the component unmounts.
    return () => {
      if (websocket) websocket.close();
      if (reconnectTimer.current) clearTimeout(reconnectTimer.current);
    };
  }, [url]);

  // Function to reset the chat for a specific room.
  // This resets stored messages, errors, and moderation history.
  const resetChat = (room: string) => {
    resetMessagesForRoom(room);
    resetErrors(room);
    resetRoom(room);
  };

  // The WebSocketContext.Provider makes these functions and state values available
  // to any child component that uses the WebSocket context.
  return (
    <WebSocketContext.Provider
      value={{
        getMessages,                    // Retrieve chat messages
        sendMessage,                    // Send a new chat message
        connectionStatus,               // Current WebSocket connection status
        rooms,                          // List of available chat rooms
        setAudioMessageListener,        // Set a listener for audio messages (from useVoice)
        requestNewRoomListener: setNewRoomListener, // Set a listener for new room requests (from useRoomStore)
        requestRoomChange: changeRoom,  // Request a room change (from useRoomStore)
        toggleVoice,                    // Toggle voice on/off (from useVoice)
        setModeratorMessageListener,    // Set a listener for moderator messages (from useModeration)
        moderationHistory,              // Stored moderator messages (from useModeration)
        errorHistory,                   // Stored error messages (from useErrorStore)
        resetChat,                      // Function to reset the chat for a room
        sendConverse,
        // Librarians store properties:
        library: currentLibrary,        // Current library (librarians) data (from useLibrariansStore)
        getLibrarianConverseMessages,
        sendList,
        getLibrarianListMessages,
        askForDocs,
        getLibrarianDocs
        }}
    >
      {children}
    </WebSocketContext.Provider>
  );
};

// Custom hook to allow easy access to the WebSocket context in other components.
export const useWebSocketContext = (): IWebSocketContext => {
  const context = useContext(WebSocketContext);
  if (!context) {
    throw new Error('useWebSocketContext must be used within a WebSocketProvider');
  }
  return context;
};
