import { WebSocketReplyChatRoomMessage } from "./WebSocketReplyChatRoomMessages";

export interface WebSocketNewRoomMessage extends WebSocketReplyChatRoomMessage {
  From: string;
  To: string;
}