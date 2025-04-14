import { useState, useRef } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketRoom } from '../models/WebSocketGetRoomsMessage';
import { WebSocketNewRoomMessage } from '../models/WebSocketNewRoomMessage';

export function useRoomStore(sender: (message: WebSocketBaseMessage) => void) {
  const [rooms, setRooms] = useState<WebSocketRoom[]>([]);
  const newRoomListenerRef = useRef<((msg: WebSocketNewRoomMessage) => void) | null>(null);

  const updateRooms = (newRooms: WebSocketRoom[]) => {
    setRooms(newRooms);
  };

  const createRoomChangedMessage = (userId:string, group: string, to: string): WebSocketBaseMessage => {
    return {
      UserId: userId,
      TransactionId: 'rooms-change-' + Date.now(),
      Action: group + '-change-room',
      SubAction: 'change',
      Content: JSON.stringify({ Group: group, To: to }),
      RoomName: '',
      SubRoomName: '',
      Mode: 'app',
    };
  };

  const createRoomResetMessage = (room: string): WebSocketBaseMessage => {
    return {
      UserId: '',
      TransactionId: 'rooms-reset-' + Date.now(),
      Action: 'rooms',
      SubAction: 'reset',
      Content: room,
      RoomName: '',
      SubRoomName: '',
      Mode: 'app',
    };
  };
  const triggerRoomsRequest = (socket:WebSocket ): void => {
    // Create a message to request the list of available rooms.
    const requestRoomsMessage: WebSocketBaseMessage = {
      UserId: '',
      TransactionId: 'rooms-get-' + Date.now(),
      Action: 'rooms',
      SubAction: 'get',
      Content: '',
      RoomName: '',
      SubRoomName: '',
      Mode: 'app',
    };
    // Send the request message.
    socket.send(JSON.stringify(requestRoomsMessage));
  }
  // Function to send a room change message.
  const changeRoom = (userId:string, group: string, to: string) => {
    const message = createRoomChangedMessage(userId, group, to);
    sender(message);
  };

  // Function to send a room reset message.
  const resetRoom = (room: string) => {
    const message = createRoomResetMessage(room);
    sender(message);
  };

  // Setter for new room listener.
  const setNewRoomListener = (listener: (msg: WebSocketNewRoomMessage) => void) => {
    newRoomListenerRef.current = listener;
  };

  // Handler for incoming new room messages.
  const handleNewRoomMessage = (msg: WebSocketNewRoomMessage) => {
    if (newRoomListenerRef.current) {
      newRoomListenerRef.current(msg);
    } else {
      console.warn('No new room listener registered.');
    }
  };

  return {
    rooms,
    updateRooms,
    createRoomChangedMessage,
    createRoomResetMessage,
    changeRoom,
    resetRoom,
    setNewRoomListener,
    handleNewRoomMessage,
    triggerRoomsRequest
  };
}
