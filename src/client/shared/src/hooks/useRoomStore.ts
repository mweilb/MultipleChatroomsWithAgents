// File: useRoomStore.ts
import { useState, useRef } from 'react';
import { WebSocketBaseMessage } from '../models/WebSocketBaseMessage';
import { WebSocketRoom } from '../models/WebSocketGetRoomsMessage';
import { WebSocketNewRoomMessage } from '../models/WebSocketNewRoomMessage';

/**
 * useRoomStore manages the list of available rooms and handles incoming "new-room" events.
 * @param sender function to send WebSocketBaseMessage over the socket
 */
export function useRoomStore(sender: (message: WebSocketBaseMessage) => void) {
  // Current list of rooms
  const [rooms, setRooms] = useState<WebSocketRoom[]>([]);

  // Ref to store the registered "new-room" listener
  const newRoomListenerRef = useRef<((msg: WebSocketNewRoomMessage) => void) | null>(null);

  /** Update the local rooms list */
  const updateRooms = (newRooms: WebSocketRoom[]) => {
    setRooms(newRooms);
  };

  /** Create a message to request the list of rooms */
  const triggerRoomsRequest = (socket: WebSocket): void => {
    const requestRoomsMessage: WebSocketBaseMessage = {
      UserId: '',
      TransactionId: `rooms-get-${Date.now()}`,
      Action: 'rooms',
      SubAction: 'get',
      Content: '',
      Mode: 'app',
    };
    socket.send(JSON.stringify(requestRoomsMessage));
  };

  /** Create and send a room-change message */
  const changeRoom = (userId: string, group: string, to: string) => {
    const msg: WebSocketBaseMessage = {
      UserId: userId,
      TransactionId: `rooms-change-${Date.now()}`,
      Action: `${group}-change-room`,
      SubAction: 'change',
      Content: JSON.stringify({ Group: group, To: to }),
      Mode: 'app',
    };
    sender(msg);
  };

  /** Create and send a room-reset message */
  const resetRoom = (room: string) => {
    const msg: WebSocketBaseMessage = {
      UserId: '',
      TransactionId: `rooms-reset-${Date.now()}`,
      Action: 'rooms',
      SubAction: 'reset',
      Content: room,
      Mode: 'app',
    };
    sender(msg);
  };

  /** Register a listener for incoming "new-room" events and return an unsubscribe */
  const setNewRoomListener = (listener: (msg: WebSocketNewRoomMessage) => void): (() => void) => {
    newRoomListenerRef.current = listener;
    return () => {
      if (newRoomListenerRef.current === listener) {
        newRoomListenerRef.current = null;
      }
    };
  };

  /** Internal handler invoked when a "new-room" message arrives */
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
    triggerRoomsRequest,
    changeRoom,
    resetRoom,
    setNewRoomListener,
    handleNewRoomMessage,
  };
}
