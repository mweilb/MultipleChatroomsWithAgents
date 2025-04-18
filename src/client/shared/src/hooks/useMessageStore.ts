import { useState, useCallback } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketReplyChatRoomMessage } from '../models/WebSocketReplyChatRoomMessages';

type NonRoomMessage = WebSocketReplyChatRoomMessage;
type ActionMessages = {
  [action: string]: {
    messages: NonRoomMessage[];
    requestState: string;
  };
};

export function useMessageStore(
  sender: (message: WebSocketBaseMessage) => void
) {
  const [messages, setMessages] = useState<ActionMessages>({});

  // Return stored messages for a given action.
  const getMessages = useCallback(
    (action: string): NonRoomMessage[] => {
      return messages[action]?.messages || [];
    },
    [messages]
  );

  // Add or update a message based on its TransactionId.
  const addOrUpdateMessage = useCallback(
    (message: WebSocketReplyChatRoomMessage): void => {

      setMessages((prevMessages: ActionMessages) => {
        const actionKey = message.Action;
        const currentEntry = prevMessages[actionKey] || {
          messages: [],
          requestState: 'complete',
        };

        // If message.SubAction === 'complete', only update requestState, do not add/update messages
        if (message.SubAction === 'completed') {
          return {
            ...prevMessages,
            [actionKey]: {
              ...currentEntry,
              requestState: 'complete',
              messages: currentEntry.messages,
            },
          };
        }

        const currentMessages = currentEntry.messages;
        const index = currentMessages.findIndex(
          (msg) => msg.TransactionId === message.TransactionId
        );
        let updatedMessages;
        if (index !== -1) {
          // Replace existing message.
          updatedMessages = currentMessages.map((msg, idx) =>
            idx === index ? message : msg
          );
        } else {
          // Add new message.
          updatedMessages = [...currentMessages, message];
        }
        const newRequestState =
          message.SubAction === 'complete' ? 'complete' : currentEntry.requestState;
        return {
          ...prevMessages,
          [actionKey]: {
            ...currentEntry,
            messages: updatedMessages,
            requestState: newRequestState,
          },
        };
      });
    },
    []
  );

  // Wrap a message from sendMessage (non-rooms only) and add it.
  const addMessageFromSend = useCallback(
    (message: WebSocketBaseMessage, orchestrator: string, room: string): void => {
      if (message.Action !== 'rooms') {
        const newMessage: WebSocketReplyChatRoomMessage = {
          ...message,
          AgentName: 'User',
          Emoji: '🤓',
          RoomName: room,
          Orchestrator: orchestrator,
        };
        addOrUpdateMessage(newMessage);
      }
    },
    [addOrUpdateMessage]
  );

  // Expose a sendMessage function that both updates the store and sends the message.
  const sendMessage = useCallback(
    (message: WebSocketBaseMessage, orchestrator: string, room: string) => {
      // Set requestState for orchestrator to 'requested'
      setMessages((prevMessages: ActionMessages) => {
        const currentEntry = prevMessages[orchestrator] || {
          messages: [],
          requestState: 'complete',
        };
        return {
          ...prevMessages,
          [orchestrator]: {
            ...currentEntry,
            requestState: 'requested',
          },
        };
      });
      // Update local store.
      addMessageFromSend(message, orchestrator, room);
      // Send the message using the provided sender function.
      sender(message);
    },
    [sender, addMessageFromSend]
  );

  // Clear stored messages for a given room and reset request status for all actions to 'complete'
  const resetMessagesForRoom = useCallback((room: string): void => {
    setMessages((prevMessages: ActionMessages) => {
      const updated = { ...prevMessages };
      if (updated[room]) {
        delete updated[room];
      }
      // Set all requestState to 'complete'
      Object.keys(updated).forEach(action => {
        updated[action] = {
          ...updated[action],
          requestState: 'complete',
        };
      });
      return updated;
    });
  }, []);

  // Get request state for a given action.
  const getRequestState = useCallback(
    (action: string): string => {
      return messages[action]?.requestState || 'complete';
    },
    [messages]
  );

  // Set request state for a given action.
  const setRequestState = useCallback(
    (action: string, state: string): void => {
      setMessages((prevMessages: ActionMessages) => {
        const currentEntry = prevMessages[action] || {
          messages: [],
          requestState: 'complete',
        };
        return {
          ...prevMessages,
          [action]: {
            ...currentEntry,
            requestState: state,
          },
        };
      });
    },
    []
  );

  return {
    messages,
    getMessages,
    addOrUpdateMessage,
    addMessageFromSend,
    resetMessagesForRoom,
    sendMessage,
    getRequestState,
    setRequestState,
  };
}
