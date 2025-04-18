// useLibrariansStore.ts
import { useState, useCallback, useRef } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketLibrariansMessage } from '../models/WebSocketGetLibrarians';
import { WebSocketLibrarianConverse } from '../models/WebSocketLibrarianConverse';
import { WebSocketLibrarianList } from '../models/WebSocketLibrarianList';


export interface LibrarianInfo {
  converse: WebSocketLibrarianConverse[];
  list: WebSocketLibrarianList[];
  docs: WebSocketLibrarianList;
}

// Define a type for storing conversation messages keyed by room and agent.
type LibrarianConverseStore = {
  [roomName: string]: {
    [agentName: string]: LibrarianInfo;
  }
};

export function useLibrariansStore(sender: (message: WebSocketBaseMessage) => void) {
  // Holds the current library data.
  const [library, setLibrary] = useState<WebSocketLibrariansMessage | null>(null);

  // Use a ref to store conversation messages for high-frequency updates.
  const librarianConverseStoreRef = useRef<LibrarianConverseStore>({});

  // A dummy state to force re-render when the ref changes.
  const [, setVersion] = useState<number>(0);

  // Handler for incoming librarian messages that contain overall library data.
  const handleLibrariansMessage = useCallback((msg: WebSocketLibrariansMessage) => {
    try {
      if (msg.Content && msg.Content.trim() !== '') {
        const parsed = JSON.parse(msg.Content) as WebSocketLibrariansMessage;
        setLibrary(parsed);
      }
    } catch (error) {
      console.error('Error parsing library content', error);
  
    }
  }, []);

  // New handler for conversation messages using a ref.
  const handleLibrarianConverseMessage = useCallback((msg: WebSocketLibrarianConverse) => {
    const { RoomName, AgentName, TransactionId } = msg;
    const store = librarianConverseStoreRef.current;

    // Get current store for the room; if it doesn't exist, initialize it.
    const roomStore = store[RoomName] || {};
    // Get current messages for the agent; if none, start with an empty array.
    const currentMessages = roomStore[AgentName] || { converse: [], list: [], docs: {} };

    // Check for an existing message with the same TransactionId.
    const messageIndex = currentMessages.converse.findIndex(m => m.TransactionId === TransactionId);
    if (messageIndex !== -1) {
      // Replace the existing message.
      currentMessages.converse[messageIndex] = msg;
    } else {
      // Otherwise, add the new message.
      currentMessages.converse.push(msg);
    }

    // Update the room store and then the entire conversation store.
    roomStore[AgentName] = currentMessages;
    store[RoomName] = roomStore;
    librarianConverseStoreRef.current = store;
    
    // Force a re-render so consumers can see the updated conversation.
    setVersion(v  => v + 1);
  }, []);

    // New handler for conversation messages using a ref.
  const handleLibrarianListMessage = useCallback((msg: WebSocketLibrarianList) => {
      const {  RoomName, AgentName, TransactionId } = msg;
      const store = librarianConverseStoreRef.current;
  
      // Get current store for the room; if it doesn't exist, initialize it.
      const roomStore = store[RoomName] || {};
      // Get current messages for the agent; if none, start with an empty array.
      const currentMessages = roomStore[AgentName] || { converse: [], list: [], docs: {} };
 
      // Check for an existing message with the same TransactionId.
      const messageIndex = currentMessages.list.findIndex(m => m.TransactionId === TransactionId);
      if (messageIndex !== -1) {
        // Replace the existing message.
        currentMessages.list[messageIndex] = msg;
      } else {
        // Otherwise, add the new message.
        currentMessages.list.push(msg);
      }
  
      // Update the room store and then the entire conversation store.
      roomStore[AgentName] = currentMessages;
      store[RoomName] = roomStore;
      librarianConverseStoreRef.current = store;
      
      // Force a re-render so consumers can see the updated conversation.
      setVersion(v => v + 1);
    }, []);


    const handleLibrarianDocMessage = useCallback((msg: WebSocketLibrarianList) => {
      const { RoomName: RoomName, AgentName } = msg;
      const store = librarianConverseStoreRef.current;
  
      // Get current store for the room; if it doesn't exist, initialize it.
      const roomStore = store[RoomName] || {};
      // Get current messages for the agent; if none, start with an empty array.
      const currentMessages = roomStore[AgentName] || { converse: [], list: [], docs: {} };
      currentMessages.docs = msg;
     
      // Update the room store and then the entire conversation store.
      roomStore[AgentName] = currentMessages;
      store[RoomName] = roomStore;
      librarianConverseStoreRef.current = store;
      
      // Force a re-render so consumers can see the updated conversation.
      setVersion(v => v + 1);
    }, []);

  // Getter function to retrieve conversation messages by room and agent.
  const getLibrarianConverseMessages = useCallback((roomName: string, agentName: string): WebSocketLibrarianConverse[] => {
    return librarianConverseStoreRef.current[roomName]?.[agentName].converse || [];
  }, []);

  const getLibrarianListMessages = useCallback((roomName: string, agentName: string): WebSocketLibrarianList[] => {
    return librarianConverseStoreRef.current[roomName]?.[agentName].list || [];
  }, []);

  const getLibrarianDocs = useCallback((roomName: string, agentName: string): WebSocketLibrarianList => {
    return librarianConverseStoreRef.current[roomName]?.[agentName].docs;
  }, []);

  // Sends a request message with action "librarians" to retrieve the library info.
  const requestLibrary = useCallback((socket: WebSocket) => {
    const message: WebSocketBaseMessage = {
      UserId: 'app',
      TransactionId: 'librarians-get-' + Date.now(),
      Action: 'librarians',
      SubAction: 'get',
      Content: '',
      Mode: 'app',
    };
    socket.send(JSON.stringify(message));
  }, []);

  // Sends a converse message to the server.
  // Here, we set Action to "librarians" and SubAction to "converse" as specified.
  const sendConverse = useCallback(
    (roomName: string, AgentName: string, text: string) => {
      const message: WebSocketBaseMessage = {
        UserId: '',
        TransactionId: 'librarians-converse-' + Date.now(),
        Action: 'librarians',
        SubAction: 'converse',
        Content: JSON.stringify({ roomName, AgentName, text }),
        Mode: 'app' 
      };
      sender(message);
    },
    [sender]
  );

  // Sends a converse message to the server.
  // Here, we set Action to "librarians" and SubAction to "converse" as specified.
  const sendList = useCallback(
    (roomName: string, AgentName: string, text: string) => {
      const message: WebSocketBaseMessage = {
        UserId: '',
        TransactionId: 'librarians-converse-' + Date.now(),
        Action: 'librarians',
        SubAction: 'list',
        Content: JSON.stringify({ roomName, AgentName, text }),
        Mode: 'app'
      };
      sender(message);
    },
    [sender]
  );


  // Sends a converse message to the server.
  // Here, we set Action to "librarians" and SubAction to "converse" as specified.
  const askForDocs = useCallback(
    (roomName: string, AgentName: string, top: number, skip: number) => {
      const message: WebSocketBaseMessage = {
        UserId: '',
        TransactionId: 'librarians-converse-' + Date.now(),
        Action: 'librarians',
        SubAction: 'docs',
        Content: JSON.stringify({ roomName, AgentName, top, skip }),
        Mode: 'app',
      };
      sender(message);
    },
    [sender]
  );

  return { 
    library, 
    requestLibrary, 
    handleLibrariansMessage, 
    sendConverse,
    handleLibrarianConverseMessage,
    getLibrarianConverseMessages,
    sendList,
    handleLibrarianListMessage,
    getLibrarianListMessages,
    askForDocs,
    handleLibrarianDocMessage,
    getLibrarianDocs
  };
}
