import { WebSocketBaseMessage } from './WebSocketBaseMessage';

export interface WebSocketNewRoomMessage extends WebSocketBaseMessage {
  From: string;
  To: string;
}