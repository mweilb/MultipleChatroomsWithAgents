import { WebSocketBaseMessage } from './WebSocketBaseMessage';

export interface WebSocketReplyChatRoomMessage extends WebSocketBaseMessage {
  Hints: { [key: string]: { [key: string]: string } };
  AgentName: string;
  Emoji: string;
}