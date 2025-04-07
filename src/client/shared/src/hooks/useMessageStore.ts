import { useState, useCallback } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketReplyChatRoomMessage } from '../models/WebSocketReplyChatRoomMessages';

type NonRoomMessage = WebSocketReplyChatRoomMessage;
type ActionMessages = { [action: string]: NonRoomMessage[] };

export function useMessageStore(
  sender: (message: WebSocketBaseMessage) => void
) {
  const [messages, setMessages] = useState<ActionMessages>({});

  // Return stored messages for a given action.
  const getMessages = useCallback(
    (action: string): WebSocketReplyChatRoomMessage[] => {
      return messages[action] || [];
    },
    [messages]
  );

  // Add or update a message based on its TransactionId.
  const addOrUpdateMessage = useCallback(
    (message: WebSocketReplyChatRoomMessage): void => {
      setMessages((prevMessages:ActionMessages) => {
        const actionKey = message.Action;
        const currentMessages = prevMessages[actionKey] || [];
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
        return { ...prevMessages, [actionKey]: updatedMessages };
      });
    },
    []
  );

  // Wrap a message from sendMessage (non-rooms only) and add it.
  const addMessageFromSend = useCallback(
    (message: WebSocketBaseMessage): void => {
      if (message.Action !== 'rooms') {
        const newMessage: WebSocketReplyChatRoomMessage = {
          ...message, 
          AgentName: 'User',
          Emoji: '🤓',
          SubRoomName: '',
          RoomName: '',
        };
        addOrUpdateMessage(newMessage);
      }
    },
    [addOrUpdateMessage]
  );

  // Expose a sendMessage function that both updates the store and sends the message.
  const sendMessage = useCallback(
    (message: WebSocketBaseMessage) => {
      // Update local store.
      addMessageFromSend(message);
      // Send the message using the provided sender function.
      sender(message);
    },
    [sender, addMessageFromSend]
  );

  // Clear stored messages for a given room.
  const resetMessagesForRoom = useCallback((room: string): void => {
    setMessages((prevMessages:ActionMessages) => {
      const updated = { ...prevMessages };
      if (updated[room]) {
        delete updated[room];
      }
      return updated;
    });
  }, []);

  return { 
    messages, 
    getMessages, 
    addOrUpdateMessage, 
    addMessageFromSend, 
    resetMessagesForRoom,
    sendMessage
  };
}
