import { WebSocketBaseMessage } from './WebSocketBaseMessage';

export interface WebSocketReplyChatRoomMessage extends WebSocketBaseMessage {
  AgentName: string;
  Emoji: string;
  
}